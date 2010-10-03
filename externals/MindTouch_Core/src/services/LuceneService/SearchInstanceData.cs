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
using System.Threading;
using log4net;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using MindTouch.Collections;
using MindTouch.IO;
using MindTouch.Tasking;
using MindTouch.Threading;
using MindTouch.Xml;

namespace MindTouch.LuceneService {

    public class SearchInstanceData : IDisposable {

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private readonly UpdateDelayQueue _queue;
        private readonly FSDirectory _directory;
        private readonly object _updateSyncroot = new object();
        private readonly ReaderWriterLockSlim _disposalLock = new ReaderWriterLockSlim();
        private readonly Analyzer _analyzer;
        private IndexWriter _writer;
        private IndexReader _reader;
        private IndexSearcher _searcher;
        private bool _searcherIsStale;
        private bool _disposed;

        //--- Constructors ---
        public SearchInstanceData(string indexPath, Analyzer analyzer, UpdateDelayQueue queue) {
            _analyzer = analyzer;
            _directory = FSDirectory.GetDirectory(indexPath);

            // Note (arnec): Needed with 2.4.0 SimpleFSLock, since a hard shutdown will have left the lock dangling
            IndexWriter.Unlock(_directory);
            try {
                _writer = new IndexWriter(_directory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            } catch(CorruptIndexException e) {
                _log.WarnFormat("The Search index at {0} is corrupt. You must repair or delete it before restarting the service. If you delete it, you must rebuild your index after service restart.", indexPath);
                if(e.Message.StartsWith("Unknown format version")) {
                    _log.Warn("The index is considered corrupt because it's an unknown version. Did you accidentally downgrade your install?");
                }
                throw;
            }
            _reader = IndexReader.Open(_directory);
            _searcher = new IndexSearcher(_reader);
            _queue = queue;
        }

        //--- Properties ---
        public int QueueSize {
            get {
                return _disposalLock.ExecuteWithReadLock(() => {
                    EnsureInstanceNotDisposed();
                    return _queue.QueueSize;
                });
            }
        }

        //--- Methods ---
        public void Enqueue(XDoc doc) {
            _disposalLock.ExecuteWithReadLock(() => {
                EnsureInstanceNotDisposed();
                _queue.Enqueue(doc);
            });
        }

        public XDoc GetStats() {
            return _disposalLock.ExecuteWithReadLock(() => {
                EnsureInstanceNotDisposed();
                var ret = new XDoc("stats");
                ret.Elem("numDocs", GetSearcher().GetIndexReader().NumDocs());
                ret.Elem("termCount", GetSearcher().GetIndexReader().GetFieldNames(IndexReader.FieldOption.ALL).Count);
                return ret;
            });
        }

        public void Dispose() {
            if(_disposed) {
                return;
            }
            _disposalLock.ExecuteWithWriteLock(() => {
                _disposed = true;
                _writer.Close();
                _searcher.Close();
                _reader.Close();
                _queue.Dispose();
            });
        }

        public void Clear() {
            _disposalLock.ExecuteWithWriteLock(() => {
                EnsureInstanceNotDisposed();
                _queue.Clear();
                _writer.Close();
                _searcher.Close();
                _reader.Close();
                _writer = new IndexWriter(_directory, _analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                _reader = IndexReader.Open(_directory);
                _searcher = new IndexSearcher(_reader);
            });
        }

        public void AddDocument(Document document) {
            _disposalLock.ExecuteWithReadLock(() => {
                EnsureInstanceNotDisposed();
                lock(_updateSyncroot) {
                    _writer.AddDocument(document);
                    _writer.Commit();
                    _searcherIsStale = true;
                }
            });
        }

        public void DeleteDocuments(Term term) {
            _disposalLock.ExecuteWithReadLock(() => {
                EnsureInstanceNotDisposed();
                lock(_updateSyncroot) {
                    _writer.DeleteDocuments(term);
                    _writer.Commit();
                    _searcherIsStale = true;
                }
            });
        }

        public Hits Search(Query query, Sort sort) {
            return _disposalLock.ExecuteWithReadLock(() => {
                EnsureInstanceNotDisposed();
                var searcher = GetSearcher();
                return sort == null ? searcher.Search(query) : searcher.Search(query, sort);
            });
        }

        private IndexSearcher GetSearcher() {
            if(_searcherIsStale) {
                lock(_updateSyncroot) {
                    if(_searcherIsStale) {
                        var reader = _reader.Reopen();
                        if(reader != _reader) {
                            _log.DebugFormat("re-opening searcher for {0}", _directory.ToString());
                            _reader.Close();
                            _searcher.Close();
                            _reader = reader;
                            _searcher = new IndexSearcher(_reader);
                        }
                        _searcherIsStale = false;
                    }
                }
            }
            return _searcher;
        }

        private void EnsureInstanceNotDisposed() {
            if(_disposed) {
                throw new ObjectDisposedException("The search instance has been disposed");
            }
        }
    }
}
