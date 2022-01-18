// BlueBrick, a LEGO(c) layout editor.
// Copyright (C) 2008 Alban NANTY
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// see http://www.fsf.org/licensing/licenses/gpl.html
// and http://www.gnu.org/licenses/
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BlueBrick
{
    class LanguageManager
    {
        private static readonly string DllName = $"{nameof(BlueBrick)}.resources.dll";
        private static readonly string ChmName = $"{nameof(BlueBrick)}.chm";

        /// <summary>
        /// This method check if the folder with the specified languageCode exist inside the starting folder
        /// of the application. If it exists it also check the presence of the language package dll as well
        /// as the HTM help file.
        /// If something is missing this method will open the dowload center to download the corresponding files
        /// from the internet.
        /// </summary>
        /// <param name="languageCode">A language code in 2 letters such as "en" or "fr"</param>
        public static void CheckLanguage(string languageCode)
        {
            // if the language is english, exit immediatly since it's the default language
            if (string.IsNullOrWhiteSpace(languageCode) || languageCode.Equals("en"))
            {
                return;
            }

            // check the presence of the folder in the startup path
            var folderPath = Application.StartupPath + @"/" + languageCode;

            try
            {
                // if it doesn't exist create it
                var languageFolder = new DirectoryInfo(folderPath);
                if (!languageFolder.Exists)
                {
                    languageFolder.Create();
                    // set the attribute to normal
                    languageFolder.Attributes = FileAttributes.Normal;
                }
            }
            catch
            {
                // we should display an error message here, saying the folder can not be created
                return;
            }

            // create a potential list of files to download
            var filesToDownload = new List<DownloadCenterForm.DownloadableFileInfo>();

            var destinationFolder = $@"/{languageCode}/";
            var url = "https://bluebrick.lswproject.com/download/language/{languageCode}/"; 

            // after checking the folder existence, check the presence of the dll package
            var languagePackage = new FileInfo(folderPath + @"/" + DllName);
            if (!languagePackage.Exists)
            {
                filesToDownload.Add(new DownloadCenterForm.DownloadableFileInfo(DllName, "1", url + DllName, destinationFolder + DllName));
            }

            // check also the presence of the chm help file for certain languages
            var languageWithHelpFile = new[] { "de", "nl", "fr", "es" };
            if (languageWithHelpFile.Contains(languageCode))
            {
                var helpFile = new FileInfo(folderPath + @"/" + ChmName);
                if (!helpFile.Exists)
                {
                    filesToDownload.Add(new DownloadCenterForm.DownloadableFileInfo(ChmName, "1", url + ChmName, destinationFolder + ChmName));
                }
            }

            // now check if the list of file to download is not null, call the download manager
            if (filesToDownload.Count > 0)
            {
                var languageManager = new DownloadCenterForm(filesToDownload, false);
                languageManager.ShowDialog();
            }
        }
    }
}
