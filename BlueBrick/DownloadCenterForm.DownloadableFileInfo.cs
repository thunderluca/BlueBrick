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

namespace BlueBrick
{
    public partial class DownloadCenterForm
    {
        /// <summary>
        /// A convenient class to sumarize the all info regarding a file to download, such as the source URL,
        /// the local destination folder, and so on.
        /// </summary>
        public class DownloadableFileInfo
        {
            public string FileName = string.Empty;
            public string Version = string.Empty;
            public string SourceURL = string.Empty;
            public string DestinationFolder = string.Empty;

            public DownloadableFileInfo()
            {
            }

            public DownloadableFileInfo(string filename, string version, string source, string destination)
            {
                FileName = filename;
                Version = version;
                SourceURL = source;
                DestinationFolder = destination;
            }
        }
    }
}