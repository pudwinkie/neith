using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace FFXIVRuby
{
    class FFXIVRuby
    {
        // Fields
        private Process _proc;
        private static string downloads_folder = "downloads";
        private static string log_folder = "log";
        private static string login_folder = "login";
        private static string mcro_file = "mcr0";
        private static string screenshots_folder = "screenshots";
        private static string ui_file = "ui";
        private static string user_folder = "user";
        private static string Zero_folder = "00000000";

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
                return Path.Combine(MyGamesFFXIV, downloads_folder);
            }
        }

        public static string FFXILoginFolderPath
        {
            get
            {
                return Path.Combine(MyGamesFFXIV, login_folder);
            }
        }

        public static string FFXIScreenshotsFolderPath
        {
            get
            {
                return Path.Combine(MyGamesFFXIV, screenshots_folder);
            }
        }

        public static string FFXIUserFolderPath
        {
            get
            {
                return Path.Combine(MyGamesFFXIV, user_folder);
            }
        }

        private static string MyGamesFFXIV
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"My Games\FINAL FANTASY XIV");
            }
        }
    }
}
