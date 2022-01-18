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
        /// a small container to provide parameters to the background worker thread
        /// </summary>
        private class DownloadParameter
		{
			public string url = string.Empty;
			public string destination = string.Empty;
			public int fileIndex = 0;
		}
	}
}