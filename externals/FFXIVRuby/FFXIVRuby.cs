using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace FFXIVRuby
{
    public class FFXIVRuby
    {
        // Fields
        private Process _proc;
        private const string downloads_folder = "downloads";
        private const string login_folder = "login";
        private const string screenshots_folder = "screenshots";
        private const string user_folder = "user";
        private const string Zero_folder = "00000000";
        private const string log_folder = "log";
        private const string mcro_file = "mcr0";
        private const string ui_file = "ui";

        // Methods
        public static FFXIVRuby Create()
        {
            var ruby = new FFXIVRuby();
            ruby._proc = FFXIVMemoryProvidor.GetFFXIVGameProcess();
            return ruby;
        }

        // Properties
        public static string FFXIDownloadsFolderPath
        {
            get
            {
                return GetFullPath(MyGamesFFXIV, downloads_folder);
            }
        }

        public static string FFXILoginFolderPath
        {
            get
            {
                return GetFullPath(MyGamesFFXIV, login_folder);
            }
        }

        public static string FFXIScreenshotsFolderPath
        {
            get
            {
                return GetFullPath(MyGamesFFXIV, screenshots_folder);
            }
        }

        public static string FFXIUserFolderPath
        {
            get
            {
                return GetFullPath(MyGamesFFXIV, user_folder);
            }
        }

        private static string MyGamesFFXIV
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"My Games\FINAL FANTASY XIV");
            }
        }

        private static string GetFullPath(params string[] args)
        {
            return Path.GetFullPath(Path.Combine(args));
        }
    }
}
