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
using System.IO;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using MindTouch.Dream;
using MindTouch.Dream.Test;
using MindTouch.Tasking;
using MindTouch.Xml;
using MindTouch.Deki.Logic;

namespace MindTouch.Deki.Tests.SiteTests
{
    [TestFixture]
    public class LicenseTests
    {
        // Necessary parameter values to generate license
        private string sn_key; // strong name key
        private string productkey; // product key
        private string gen_license; // destination file for license generator (i.e. out=gen_license)
        private string license; // path to the deki license
        private string host_address; // host address
        private XDoc oldlicensedoc; // user's existing license

        // Reuseable, generic licenses
        string[] community;
        string[] trial;
        string[] commercial;
        string[] expired;

        [TestFixtureSetUp]
        public void FixtureSetUp() {
            // Retrieve values from mindtouch.deki.tests.xml
            sn_key = Utils.Settings.AssetsPath + "/keys/mindtouch.snk";
            gen_license = Utils.Settings.StorageDir + "/license-" + DateTime.Now.Ticks.ToString() + ".xml";
            productkey = Utils.Settings.ProductKey;
            host_address = Utils.Settings.HostAddress;

            if (!File.Exists(sn_key))
                Assert.Fail("Could not find strong name key (.snk) file.");
            if (productkey == "badkey")
                Assert.Fail("Could not find product key.");
            if (Utils.Settings.StorageDir == null)
                Assert.Fail("bin path required.");
            if (File.Exists(gen_license))
                Assert.Fail("A license file at the given path already exists: " + gen_license);

            // Store existing license
            license = Utils.Settings.StorageDir + "/_x002F_deki/license.xml";
            oldlicensedoc = LicenseFileToDoc(license);

            // Initialize arguments for generic licenses
            community = new string[] { "type=community",
                                       "sign=" + sn_key,
                                       "out=" + gen_license };

            trial = new string[] { "type=trial",
                                   "sign=" + sn_key,
                                   "productkey=" + productkey,
                                   "name=foo",
                                   "email=foo@mindtouch.com",
                                   "out=" + gen_license};

            commercial = new string[] { "type=commercial",
                                        "sign=" + sn_key,
                                        "id=123",
                                        "productkey=" + productkey,
                                        "licensee=Acme",
                                        "address=123",
                                        "hosts=" + host_address,
                                        "name=foo",
                                        "phone=123-456-7890",
                                        "email=foo@mindtouch.com",
                                        "users=infinite",
                                        "sites=infinite",
                                        "out=" + gen_license };

            expired = new string[] { "type=trial",
                                     "sign=" + sn_key,
                                     "productkey=" + productkey,
                                     "name=foo",
                                     "email=foo@mindtouch.com",
                                     "expiration=now+1",
                                     "out=" + gen_license };
        }

        [TestFixtureTearDown]
        public void FixtureTearDown() {
            // Delete temporary generated license
            if (System.IO.File.Exists(gen_license)) {
                System.IO.File.Delete(gen_license);
            }

            // Delete any test license (if exists)
            DeleteLicense();

            // Restore license saved in fixture set up
            if (oldlicensedoc != null) {
                SaveLicense(oldlicensedoc);
            }

            // Restart host
            Utils.Settings.ShutdownHost();
        }

        [SetUp]
        public void SetUp() {
            // Set clean license state prior to every test
            // Delete license + Restart host
            DeleteLicense();
            Utils.Settings.ShutdownHost();
        }

        [Test]
        public void NoLicense_RevertsToCommunity() {
            // Retrieve license
            Plug p = Utils.BuildPlugForAdmin();
            DreamMessage msg = GetLicenseXML(p);
            Assert.AreEqual("community", msg.ToDocument()["@type"].AsText ?? String.Empty, "License did not revert back to community.");
        }

        [Test] 
        public void RetriveLicenseAsAnonymousAndAdmin() {
            // Assure both private and public licenses are output
            Plug p = Utils.BuildPlugForAdmin();
            DreamMessage msg = GetLicenseXML(p);
            Assert.IsTrue(!msg.ToDocument()["//license.public"].IsEmpty, "license.public element is not present!");
            Assert.IsTrue(!msg.ToDocument()["//license.private"].IsEmpty, "license.private element is not present!");

            // Only public license should be output
            p = Utils.BuildPlugForAnonymous();
            msg = GetLicenseXML(p);
            Assert.IsTrue(!msg.ToDocument()["//license.public"].IsEmpty, "license.public element is not present!");
            Assert.IsTrue(msg.ToDocument()["//license.private"].IsEmpty, "license.private element is present!");
        }

        [Test]
        public void SaveCommunityLicense() {
            // Generate a community license
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(community);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));

            // Retrieve generated license
            XDoc license = LicenseFileToDoc(gen_license);

            // Save community license and start deki service
            SaveLicense(license);
            Plug p = Utils.BuildPlugForAdmin();

            // Retrieve license
            DreamMessage msg = GetLicenseXML(p);
            Assert.AreEqual("community", msg.ToDocument()["@type"].AsText ?? String.Empty, "Unexpected license type.");
        }

        [Test]
        public void SaveTrialLicense() {
            // Generate a trial license
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(trial);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));

            // Retrieve generated license
            XDoc license = LicenseFileToDoc(gen_license);

            // Save trial license and start deki service
            SaveLicense(license);
            Plug p = Utils.BuildPlugForAdmin();

            // Retrieve license
            DreamMessage msg = GetLicenseXML(p);
            Assert.AreEqual("trial", msg.ToDocument()["@type"].AsText ?? String.Empty, "Unexpected license type.");
        }

        [Test]
        public void PutCommercialLicense() {
            // Generate the ultimate commercial license
            string[] args = new string[] { "type=commercial",
                                           "sign=" + sn_key,
                                           "id=123",
                                           "productkey=" + productkey,
                                           "licensee=Acme",
                                           "address=123",
                                           "hosts=" + host_address,
                                           "name=foo",
                                           "phone=123-456-7890",
                                           "email=foo@mindtouch.com",
                                           "users=infinite",
                                           "sites=infinite",
                                           "expiration=never",
                                           "sid=sid://mindtouch.com/ent",
                                           "sid=sid://mindtouch.com/std/",
                                           "sidexpiration=never",
                                           "sid=sid://mindtouch.com/ext/2009/12/anychart",
                                           "sidexpiration=now+600",
                                           "sid=sid://mindtouch.com/ext/2009/12/anygantt",
                                           "sidexpiration=90",
                                           "sid=sid://mindtouch.com/ext/2010/06/analytics.content",
                                           "sidexpiration=" + String.Format("{0:yyyyMMdd}", DateTime.Now.AddDays(90)),
                                           "sid=sid://mindtouch.com/ext/2010/06/analytics.search",
                                           "capability:shared-cache-provider=memcache",
                                           "capabilityexpiration=" + String.Format("{0:yyyyMMdd}", DateTime.Now.AddDays(180)),
                                           "capability:anonymous-permissions=ALL",
                                           "capabilityexpiration=60",
                                           "capability:search-engine=adaptive",
                                           "capabilityexpiration=never",
                                           "capability:content-rating=enabled",
                                           "capabilityexpiration=now+600",
                                           "out=" + gen_license };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));

            // Retrieve generated license
            XDoc license = LicenseFileToDoc(gen_license);

            // Upload community license via API
            Plug p = Utils.BuildPlugForAdmin();
            DreamMessage msg = p.At("license").Put(license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Uploading commercial license failed!");
        }

        [Test]
        public void PutCommercialLicenseWithUserLimitLessThanAlreadyExist() {
            // Log in as ADMIN
            Plug p = Utils.BuildPlugForAdmin();

            // Retrieve # of active users
            DreamMessage msg = p.At("users").With("activatedfilter", "true").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Users retrieval failed");
            int usercount = msg.ToDocument()["@querycount"].AsInt ?? 0;
            Assert.IsTrue(usercount > 0, "No active users?");

            // Generate a commercial license
            usercount -= 2;
            string[] args = new string[] { "type=commercial",
                                           "sign=" + sn_key,
                                           "id=123",
                                           "productkey=" + productkey,
                                           "licensee=Acme",
                                           "address=123",
                                           "hosts=" + host_address,
                                           "name=foo",
                                           "phone=123-456-7890",
                                           "email=foo@mindtouch.com",
                                           "users=" + usercount,
                                           "sites=infinite",
                                           "out=" + gen_license };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));

            // Retrieve generated license
            XDoc license = LicenseFileToDoc(gen_license);

            // Upload community license via API
            msg = p.At("license").Put(license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Forbidden, msg.Status, "Uploading commercial license with less users than currently exist succeeded?!");
        }

        [Test]
        public void CreateMoreUsersThanLicenseAllows() {
            // Log in as ADMIN
            Plug p = Utils.BuildPlugForAdmin();

            // Retrieve # of active users
            DreamMessage msg = p.At("users").With("activatedfilter", "true").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Users retrieval failed");
            int usercount = msg.ToDocument()["@querycount"].AsInt ?? 0;
            Assert.IsTrue(usercount > 0, "No active users?");

            // Create and retrieve commercial license for active users
            string[] args = new string[] { "type=commercial",
                                           "sign=" + sn_key,
                                           "id=123",
                                           "productkey=" + productkey,
                                           "licensee=Acme",
                                           "address=123",
                                           "hosts=" + host_address,
                                           "name=foo",
                                           "phone=123-456-7890",
                                           "email=foo@mindtouch.com",
                                           "users=" + usercount,
                                           "sites=infinite",
                                           "out=" + gen_license };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            XDoc license = LicenseFileToDoc(gen_license);

            // Upload license
            msg = p.At("license").Put(license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Uploading commercial license failed!");

            // Create a user (should succeed)
            msg = CreateUser(p);
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Creating a user within license limit failed?!");

            // Create another user (should fail)
            msg = CreateUser(p);
            Assert.AreEqual(DreamStatus.Forbidden, msg.Status, "Creating more users than allowed by license succeeded?!");
        }

        [Test]
        public void PutTrialLicenseToExpire() {
            // Generate a trial license to expire in 2 seconds
            string[] args = new string[] { "type=trial",
                                           "sign=" + sn_key,
                                           "productkey=" + productkey,
                                           "name=foo",
                                           "email=foo@mindtouch.com",
                                           "expiration=now+2",
                                           "out=" + gen_license };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));

            // Retrieve generated license
            XDoc license = LicenseFileToDoc(gen_license);

            // Save trial license
            SaveLicense(license);

            // Wait for 2 seconds
            WaitFor(2);

            // Start the deki service
            Plug p = Utils.BuildPlugForAdmin();

            // Retrieve license
            DreamMessage msg = GetLicenseXML(p);
            Assert.AreEqual("trial", msg.ToDocument()["@type"].AsText ?? String.Empty, "Unexpected license type.");

            // Retrieve settings and verify that license/state = EXPIRED
            msg = p.At("site", "settings").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual("EXPIRED", msg.ToDocument()["license/state"].AsText ?? String.Empty, "Unexpected license state in site/settings");
        }

        [Test]
        public void PutCommercialLicense_AnonymousAccessCapabilityToExpire() {
            const int CAPABILITY_EXPIRATION = 10;

            // Log in as ADMIN
            Plug p = Utils.BuildPlugForAdmin();

            // Create a commercial license where anonymous access expires
            string[] args = new string[] { "type=commercial",
                                           "sign=" + sn_key,
                                           "id=123",
                                           "productkey=" + productkey,
                                           "licensee=Acme",
                                           "address=123",
                                           "hosts=" + host_address,
                                           "name=foo",
                                           "phone=123-456-7890",
                                           "email=foo@mindtouch.com",
                                           "users=infinite",
                                           "sites=infinite",
                                           "capability:anonymous-permissions=ALL",
                                           "capabilityexpiration=now+" + CAPABILITY_EXPIRATION,
                                           "out=" + gen_license };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            XDoc license = LicenseFileToDoc(gen_license);

            // Upload license
            DreamMessage msg = p.At("license").Put(license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Uploading commercial license failed!");

            // Retrieve home page as anonymous
            msg = p.At("pages", "home").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Anonymous page retrieval failed?!");

            // Wait CAPABILITY_EXPIRATION seconds
            WaitFor(CAPABILITY_EXPIRATION);

            // Restart service
            Utils.Settings.ShutdownHost();
            p = Utils.BuildPlugForAnonymous();

            // Retrieve home page as anonymous again, should be unauthorized
            msg = p.At("pages", "home").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Unauthorized, msg.Status, "Anonymous still has READ permissions?!");
        }

        [Test]
        public void TestLicenseTransitions() {
            // 1. Delete license
            // 2. Generate and save license to transition from
            // 3. Restart host
            // 4. Upload new license

            // Commercial license to transition to
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(commercial);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            XDoc commercial_license = LicenseFileToDoc(gen_license);

            // Community -> Commercial
            Plug p = Utils.BuildPlugForAdmin();
            DreamMessage msg = p.At("license").Put(commercial_license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Community -> Commercial transition failed");

            // Trial -> Commercial
            DeleteLicense();
            exitValues = GenerateLicense(trial);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            XDoc license = LicenseFileToDoc(gen_license);
            SaveLicense(license);
            Utils.Settings.ShutdownHost();
            p = Utils.BuildPlugForAdmin();
            msg = p.At("license").Put(commercial_license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Trial -> Commercial transition failed");

            // Commercial -> Commercial
            DeleteLicense();
            exitValues = GenerateLicense(commercial);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            license = LicenseFileToDoc(gen_license);
            SaveLicense(license);
            Utils.Settings.ShutdownHost();
            p = Utils.BuildPlugForAdmin();
            msg = p.At("license").Put(commercial_license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Commercial -> Commercial transition failed");

            // Expired -> Commercial
            DeleteLicense();
            exitValues = GenerateLicense(expired);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            license = LicenseFileToDoc(gen_license);
            SaveLicense(license);
            Utils.Settings.ShutdownHost();
            p = Utils.BuildPlugForAdmin();
            msg = p.At("license").Put(commercial_license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Expired -> Commercial transition failed");

            // * -> Expired
            msg = p.At("license").Put(license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.BadRequest, msg.Status, "Transition to expired license succeeded?!");
        }

        [Test]
        public void CommunityToCommercialWithBadProductKey_BadRequest() {
            // Commercial license to transition to
            string[] args = new string[] { "type=commercial",
                                           "sign=" + sn_key,
                                           "id=123",
                                           "productkey=00000000000000000000000000000000",
                                           "licensee=Acme",
                                           "address=123",
                                           "hosts=" + host_address,
                                           "name=foo",
                                           "phone=123-456-7890",
                                           "email=foo@mindtouch.com",
                                           "users=infinite",
                                           "sites=infinite",
                                           "out=" + gen_license };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            XDoc commercial_license = LicenseFileToDoc(gen_license);

            // Community -> Commercial
            Plug p = Utils.BuildPlugForAdmin();
            DreamMessage msg = p.At("license").Put(commercial_license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.BadRequest, msg.Status, "Community -> Commercial transition with bad product key succeeded?");
        }

        [Test]
        public void Upload_Save_BadLicense_AndCreateUser() {
            // Generate a commercial license
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(commercial);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            XDoc license = LicenseFileToDoc(gen_license);

            // Tamper with the XML to invalidate it
            license["@type"].ReplaceValue("invalid");

            // Attempt to upload tampered license
            Plug p = Utils.BuildPlugForAdmin();
            DreamMessage msg = p.At("license").Put(license, new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.BadRequest, msg.Status, "Upload of tampered license succeeded?!");

            // Sidestep API by saving license and restarting service
            DeleteLicense();
            SaveLicense(license);
            Utils.Settings.ShutdownHost();

            // Check that the license state is invalid
            p = Utils.BuildPlugForAdmin();
            msg = p.At("site", "settings").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual("INVALID", msg.ToDocument()["license/state"].AsText ?? String.Empty, "Unexpected license state in site/settings");

            // Try to create a user
            msg = CreateUser(p);
            Assert.AreEqual(DreamStatus.Forbidden, msg.Status, "User creation with invalid license succeeded?!");
        }

        [Test]
        public void SaveExpiredCommercialLicenseWithinAndBeyondGracePeriod() {
            // Hard coded in LicenseBL.cs
            const int GRACE = 14;

            // Within grace (GRACE/2)
            string[] args = new string[] { "type=commercial",
                                           "sign=" + sn_key,
                                           "id=123",
                                           "productkey=" + productkey,
                                           "licensee=Acme",
                                           "address=123",
                                           "hosts=" + host_address,
                                           "name=foo",
                                           "phone=123-456-7890",
                                           "email=foo@mindtouch.com",
                                           "users=infinite",
                                           "sites=infinite",
                                           "expiration=" + String.Format("{0:yyyyMMdd}", DateTime.Now.AddDays(-GRACE/2)),
                                           "out=" + gen_license };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            XDoc license = LicenseFileToDoc(gen_license);

            // Need to sidestep API since uploading expired license is not allowed
            SaveLicense(license);

            // Check that license is not yet expired
            Plug p = Utils.BuildPlugForAdmin();
            DreamMessage msg = p.At("site", "settings").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual("COMMERCIAL", msg.ToDocument()["license/state"].AsText ?? String.Empty, "Unexpected license state in site/settings");

            // Generate license outside of grace (GRACE*2)
            args = new string[] { "type=commercial",
                                           "sign=" + sn_key,
                                           "id=123",
                                           "productkey=" + productkey,
                                           "licensee=Acme",
                                           "address=123",
                                           "hosts=" + host_address,
                                           "name=foo",
                                           "phone=123-456-7890",
                                           "email=foo@mindtouch.com",
                                           "users=infinite",
                                           "sites=infinite",
                                           "expiration=" + String.Format("{0:yyyyMMdd}", DateTime.Now.AddDays(-GRACE*2)),
                                           "out=" + gen_license };
            exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            license = LicenseFileToDoc(gen_license);

            // Need to sidestep API since uploading expired license is not allowed
            DeleteLicense();
            SaveLicense(license);
            Utils.Settings.ShutdownHost();

            // Check that license is expired
            p = Utils.BuildPlugForAdmin();
            msg = p.At("site", "settings").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual("EXPIRED", msg.ToDocument()["license/state"].AsText ?? String.Empty, "Unexpected license state in site/settings");
        }

        [Test]
        public void CommercialSaveOldVersion_IsExpired() {
            // Save version 8
            string[] args = new string[] { "type=commercial",
                                           "sign=" + sn_key,
                                           "id=123",
                                           "productkey=" + productkey,
                                           "licensee=Acme",
                                           "address=123",
                                           "hosts=" + host_address,
                                           "name=foo",
                                           "phone=123-456-7890",
                                           "email=foo@mindtouch.com",
                                           "users=infinite",
                                           "sites=infinite",
                                           "expiration=never",
                                           "version=8",
                                           "out=" + gen_license };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
            XDoc license = LicenseFileToDoc(gen_license);

            // Need to sidestep API since uploading expired license is not allowed
            SaveLicense(license);

            // Check that license is expired
            Plug p = Utils.BuildPlugForAdmin();
            DreamMessage msg = p.At("site", "settings").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual("EXPIRED", msg.ToDocument()["license/state"].AsText ?? String.Empty, "Unexpected license state in site/settings");
        }

        [Test]
        public void TestLicenseGenerator() {
            // This test method invokes GenerateLicense() which runs the license generator
            // tool. Command line arguments are passed as an array of strings.
            // The purpose of this method tests only the license generator and checks
            // whether a license was successfully generated or not. 

            // No args - returns error
            string[] args = new string[] { };
            Tuplet<int, Stream, Stream> exitValues = GenerateLicense(args);
            Assert.AreNotEqual(0, exitValues.Item1, "Unexpected return code");

            // Community
            exitValues = GenerateLicense(community);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));

            // Trial
            exitValues = GenerateLicense(trial);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));

            // Commercial
            exitValues = GenerateLicense(commercial);
            Assert.AreEqual(0, exitValues.Item1, "Unexpected return code\n" + GetErrorMsg(exitValues.Item2) + GetErrorMsg(exitValues.Item3));
        }

        private Tuplet<int, Stream, Stream> GenerateLicense(string[] args) {
            string licenseGenerator = Utils.Settings.AssetsPath + "/deki.license/mindtouch.license.exe";

            if (!System.IO.File.Exists(licenseGenerator)) {
                Assert.Fail("Invalid path to license generator.");
            }

            const int TIME_OUT = 60000;
            string cmdlineargs = String.Empty;

            // arrange command line arguments
            for (int i = 0; i < args.Length; i++) {
                if (i == (args.Length - 1))
                    cmdlineargs += "\"" + args[i] + "\"";
                else
                    cmdlineargs += "\"" + args[i] + "\" ";
            }

            // Run the license generator
            return Async.ExecuteProcess(licenseGenerator, 
                                        cmdlineargs, 
                                        null, 
                                        new Result<Tuplet<int, Stream, Stream>>(TimeSpan.FromMilliseconds(TIME_OUT)))
                                        .Wait();
        }

        // Save license at license storage path
        private void SaveLicense(XDoc license_doc) {
            FileStream fs = new FileStream(license, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(license_doc.ToString());
            sw.Close();
        }

        // Delete license from license storage path
        private void DeleteLicense() {
            if (System.IO.File.Exists(license)) {
                System.IO.File.Delete(license);
            }
        }

        // Error stream -> error string
        private string GetErrorMsg(Stream error) {
            StreamReader sr = new StreamReader(error);
            return sr.ReadToEnd();
        }

        // Convert license file to XDoc
        private XDoc LicenseFileToDoc(string file) {
            if (!System.IO.File.Exists(file)) {
                return null;
            }
            return XDocFactory.LoadFrom(file, MimeType.XML);
        }

        // GET:license request with success check
        private DreamMessage GetLicenseXML(Plug p) {
            DreamMessage msg = p.At("license").Get(new Result<DreamMessage>()).Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "License retrieval failed");
            return msg;
        }

        // Create a user
        private DreamMessage CreateUser(Plug p) {
            // User XML
            string name = Utils.GenerateUniqueName();
            string role = "Contributor";
            string email = "licensetest@mindtouch.com";
            string password = "password";
            XDoc usersDoc = new XDoc("user")
                .Elem("username", name)
                .Elem("email", email)
                .Elem("fullname", name + "'s full name")
                .Start("permissions.user")
                    .Elem("role", role)
                .End();

            // Send request to create user
            return p.At("users").With("accountpassword", password).Post(usersDoc, new Result<DreamMessage>()).Wait();
        }

        // Wait for N seconds
        private void WaitFor(int seconds) {
            Wait.For(() =>
                {
                    return false;
                }, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        ///     Retrieve site license
        /// </summary>        
        /// <feature>
        /// <name>GET:license</name>
        /// <uri>http://developer.mindtouch.com/en/ref/MindTouch_API/GET%3alicense</uri>
        /// </feature>
        /// <expected>200 OK HTTP response</expected>

        [Test]
        public void GetLicense() {
            // Build ADMIN plug
            Plug p = Utils.BuildPlugForAdmin();

            // Retrieve license in XML format
            DreamMessage msg = p.At("license").With("format", "xml").Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "License retrieval in XML format failed");

            // Retrieve license in HTML format
            msg = p.At("license").With("format", "html").Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "License retrieval in HTML format failed");
        }

    }
}
