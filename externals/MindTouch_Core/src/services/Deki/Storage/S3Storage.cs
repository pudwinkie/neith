/*
 * MindTouch Core - open source enterprise collaborative networking
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using MindTouch.Deki.Data;
using MindTouch.Dream;
using MindTouch.IO;
using MindTouch.Tasking;
using MindTouch.Xml;

namespace MindTouch.Deki.Storage {
    public class S3Storage : IStorageProvider {

        //--- Constants ---
        private const string AWS_DATE = "X-Amz-Date";
        private const int CACHE_TTL = 60 * 60;
        private const double DEFAUTL_S3_TIMEOUT = 30;

        //--- Class Methods ---
        private static void SafeFileDelete(string filename) {
            if(File.Exists(filename)) {
                try {
                    File.Delete(filename);
                } catch { }
            }
        }

        //--- Fields ---
        private string _public_key;
        private string _private_key;
        private string _bucket;
        private Plug _s3;
        private string _prefix;
        private Dictionary<string, Tuplet<string, TaskTimer, DateTime?>> _cache = new Dictionary<string, Tuplet<string, TaskTimer, DateTime?>>();
        private bool _allowRedirects;
        private TimeSpan _redirectTimeout;

        //--- Constructors ---
        public S3Storage(XDoc configuration) {
            _public_key = configuration["publickey"].AsText;
            _private_key = configuration["privatekey"].AsText;
            _bucket = configuration["bucket"].AsText;
            _prefix = configuration["prefix"].AsText;
            if(string.IsNullOrEmpty(_public_key)) {
                throw new ArgumentException("Invalid Amazon S3 publickey");
            }
            if(string.IsNullOrEmpty(_private_key)) {
                throw new ArgumentException("Invalid Amazon S3 privatekey");
            }
            if(string.IsNullOrEmpty(_bucket)) {
                throw new ArgumentException("Invalid Amazon S3 bucket");
            }
            if(string.IsNullOrEmpty(_prefix)) {
                throw new ArgumentException("Invalid Amazon S3 prefix");
            }

            _allowRedirects = configuration["allowredirects"].AsBool ?? false;
            _redirectTimeout = TimeSpan.FromSeconds(configuration["redirecttimeout"].AsInt ?? 60);

            // initialize S3 plug
            _s3 = Plug.New("http://s3.amazonaws.com", TimeSpan.FromSeconds(configuration["timeout"].AsDouble ?? DEFAUTL_S3_TIMEOUT)).WithPreHandler(S3AuthenticationHeader).At(_bucket);
        }

        //--- Methods ---
        public StreamInfo GetFile(AttachmentBE attachment, SizeType size, bool allowFileLink) {
            MimeType mime;

            switch(size) {
            case SizeType.THUMB:
            case SizeType.WEBVIEW:
                mime = Deki.Logic.AttachmentPreviewBL.ResolvePreviewMime(attachment.MimeType);
                break;
            default:
                mime = attachment.MimeType;
                break;
            }

            return GetFileInternal(BuildS3Filename(attachment, size), mime, allowFileLink);
        }

        public void PutFile(AttachmentBE attachment, SizeType size, StreamInfo file) {
            PutFileInternal(BuildS3Filename(attachment, size), attachment.Name, file);
        }

        public void MoveFile(AttachmentBE attachment, PageBE targetPage) {
            //Nothing to do here.
        }

        public void DeleteFile(AttachmentBE attachment, SizeType size) {
            DeleteFileInternal(BuildS3Filename(attachment, size));
        }

        public void PutSiteFile(string label, StreamInfo file) {
            PutFileInternal(BuildS3SiteFilename(label), string.Empty, file);
        }

        public DateTime GetSiteFileTimestamp(string label) {
            return GetFileTimeStampInternal(BuildS3SiteFilename(label));
        }

        public StreamInfo GetSiteFile(string label, bool allowFileLink) {
            return GetFileInternal(BuildS3SiteFilename(label), MimeType.FromFileExtension(label), allowFileLink);
        }

        public void DeleteSiteFile(string label) {
            DeleteFileInternal(BuildS3SiteFilename(label));
        }

        public void Shutdown() {

            // copy list of cached files
            List<Tuplet<string, TaskTimer, DateTime?>> entries = new List<Tuplet<string, TaskTimer, DateTime?>>(_cache.Count);
            lock(_cache) {
                entries.AddRange(_cache.Values);
                _cache.Clear();
            }

            // delete all cached files
            foreach(Tuplet<string, TaskTimer, DateTime?> entry in entries) {
                entry.Item2.Cancel();
                SafeFileDelete(entry.Item1);
            }
        }

        private StreamInfo GetFileInternal(string filename, MimeType type, bool allowFileLink) {

            if(allowFileLink && _allowRedirects) {
                return new StreamInfo(BuildS3Uri(Verb.GET, _s3.AtPath(filename), _redirectTimeout));
            }

            // check if file is cached
            Tuplet<string, TaskTimer, DateTime?> entry = GetCachedEntry(filename);
            if(entry != null) {
                Stream filestream = File.Open(entry.Item1, FileMode.Open, FileAccess.Read, FileShare.Read);
                return new StreamInfo(filestream, filestream.Length, type, entry.Item3);
            }

            // get file from S3
            Result<DreamMessage> result = new Result<DreamMessage>();
            _s3.AtPath(filename).InvokeEx(Verb.GET, DreamMessage.Ok(), result);
            DreamMessage response = result.Wait();
            try {
                if(response.IsSuccessful) {
                    return new StreamInfo(response.AsStream(), response.ContentLength, response.ContentType, GetLastModifiedTimestampFromResponse(response));
                } else if(response.Status == DreamStatus.NotFound) {
                    response.Close();
                    return null;
                } else {
                    throw new DreamInternalErrorException(string.Format("S3 unable to fetch file (status {0}, message {1})", response.Status, response.AsText()));
                }
            } catch {
                if(response != null) {
                    response.Close();
                }
                throw;
            }
        }

        private void PutFileInternal(string s3Filename, string filename, StreamInfo file) {
            string tmpfile;
            using(file) {
                tmpfile = Path.GetTempFileName();

                // create tmp file
                try {

                    // copy stream to tmp file
                    using(Stream stream = File.Create(tmpfile)) {
                        file.Stream.CopyTo(stream, file.Length, new Result<long>(TimeSpan.MaxValue)).Wait();
                    }

                    // create cached entry
                    TaskTimer timer = TaskTimer.New(TimeSpan.FromSeconds(CACHE_TTL), OnTimer, s3Filename, TaskEnv.None);
                    lock(_cache) {

                        // cach everything we know about the file
                        _cache[s3Filename] = new Tuplet<string, TaskTimer, DateTime?>(tmpfile, timer, file.Modified);
                    }
                } catch(Exception e) {
                    try {

                        // delete tmp file if it exists
                        SafeFileDelete(tmpfile);
                        lock(_cache) {
                            _cache.Remove(s3Filename);
                        }
                    } catch { }
                    throw new DreamInternalErrorException(string.Format("Unable to cache file attachment to '{0}' ({1})", s3Filename, e.Message));
                }
            }

            // forward cached file to S3
            Stream filestream = File.Open(tmpfile, FileMode.Open, FileAccess.Read, FileShare.Read);
            file = new StreamInfo(filestream, file.Length, file.Type);
            DreamMessage s3Msg = DreamMessage.Ok(file.Type, file.Length, file.Stream);
            s3Msg.Headers.ContentDisposition = new ContentDisposition(true, DateTime.UtcNow, null, null, filename, file.Length);

            // Note (arnec): The timeout is just a workaround Plug not having some kind of heartbeat on progress. Ideally 30 seconds of inactivity
            // should be perfectly fine, as long as we track uploads that are proceeding as active
            _s3.AtPath(s3Filename).WithTimeout(TimeSpan.FromMinutes(30)).Put(s3Msg);
        }

        private void DeleteFileInternal(string filename) {
            RemoveCachedEntry(filename, false);
            _s3.AtPath(filename).DeleteAsync().Wait();
        }

        private DateTime GetFileTimeStampInternal(string filename) {

            // check cache
            Tuplet<string, TaskTimer, DateTime?> entry = GetCachedEntry(filename);
            if(entry != null) {
                return entry.Item3 ?? DateTime.MinValue;
            }

            // get file information from S3
            DreamMessage response = _s3.AtPath(filename).InvokeAsync("HEAD", DreamMessage.Ok()).Wait();
            return GetLastModifiedTimestampFromResponse(response);
        }

        private DateTime GetLastModifiedTimestampFromResponse(DreamMessage response) {
            if(response.IsSuccessful) {
                return response.Headers.LastModified ?? DateTime.MinValue;
            }
            return DateTime.MinValue;
        }

        private string BuildS3Filename(AttachmentBE attachment, SizeType size) {
            string id;

            //Legacy pre-Lyons S3 paths are based on fileid. If the fileid is present in the resource then use it otherwise base on resourceid.
            if(attachment.FileId == null) {
                id = string.Format("r{0}", attachment.ResourceId);
            } else {
                id = attachment.FileId.ToString();
            }

            switch(size) {
            case SizeType.THUMB:
            case SizeType.WEBVIEW:
                return string.Format("{0}/{1}/{2}/{3}", _prefix, id, attachment.Content.Revision - 1, size.ToString().ToLowerInvariant());
            default:
                return string.Format("{0}/{1}/{2}", _prefix, id, attachment.Content.Revision - 1);
            }
        }

        private string BuildS3SiteFilename(string label) {
            return string.Format("{0}/{1}/{2}", _prefix, "SITE", label);
        }

        private DreamMessage S3AuthenticationHeader(string verb, XUri uri, XUri normalizedUri, DreamMessage message) {

            // add amazon date header
            string date = DateTime.UtcNow.ToString("r");
            message.Headers[AWS_DATE] = date;

            // add authorization header
            string result = string.Format("{0}\n{1}\n{2}\n\n{3}:{4}\n{5}", verb, message.Headers[DreamHeaders.CONTENT_MD5], message.ContentType, AWS_DATE.ToLowerInvariant(), date, normalizedUri.Path);
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_private_key));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(result)));
            message.Headers.Authorization = string.Format("AWS {0}:{1}", _public_key, signature);
            message.Headers.ContentType = message.ContentType;
            return message;
        }

        private XUri BuildS3Uri(string verb, XUri uri, TimeSpan expireTime) {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            string expireTimeSeconds = expireTimeSeconds = ((long)(new TimeSpan(DateTime.UtcNow.Add(expireTime).Subtract(epoch).Ticks).TotalSeconds)).ToString();

            string result = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", verb, string.Empty, string.Empty, expireTimeSeconds, uri.Path);
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_private_key));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(result)));

            XUri ret = uri.With("AWSAccessKeyId", _public_key).With("Signature", signature).With("Expires", expireTimeSeconds);
            return ret;
        }

        private void OnTimer(TaskTimer timer) {
            RemoveCachedEntry((string)timer.State, true);
        }

        private Tuplet<string, TaskTimer, DateTime?> GetCachedEntry(string filename) {
            Tuplet<string, TaskTimer, DateTime?> result;
            lock(_cache) {
                if(_cache.TryGetValue(filename, out result)) {
                    if(File.Exists(result.Item1)) {
                        result.Item2.Change(TimeSpan.FromSeconds(CACHE_TTL), TaskEnv.Current);
                    } else {
                        result.Item2.Cancel();
                        _cache.Remove(filename);
                        return null;
                    }
                }
            }
            return result;
        }

        private void RemoveCachedEntry(string filename, bool timeout) {
            Tuplet<string, TaskTimer, DateTime?> entry;
            lock(_cache) {
                if(_cache.TryGetValue(filename, out entry)) {

                    // avoid race condition
                    if(timeout) {
                        if(entry.Item2.When != DateTime.MaxValue) {
                            return;
                        }
                    } else {
                        entry.Item2.Cancel();
                    }
                    _cache.Remove(filename);
                }
            }
            if(entry != null) {
                SafeFileDelete(entry.Item1);
            }
        }
    }
}
