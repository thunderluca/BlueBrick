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

using System;
using System.Collections.Generic;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Maps
{
	class ChangeGeneralInfo : Action
	{
		private class GeneralMapInfo
		{
			public string mAuthor = null;
			public string mLUG = null;
			public string mEvent = null;
			public DateTime mDate;
			public string mComment = null;
			public override bool Equals(object obj)
			{
                if (obj is GeneralMapInfo other)
                    return other.mAuthor.Equals(this.mAuthor) && other.mLUG.Equals(this.mLUG) && other.mEvent.Equals(this.mEvent) && other.mDate.Equals(this.mDate) && other.mComment.Equals(this.mComment);
                return false;
			}

            public override int GetHashCode()
            {
                int hashCode = -1319741560;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(mAuthor);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(mLUG);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(mEvent);
                hashCode = hashCode * -1521134295 + mDate.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(mComment);
                return hashCode;
            }
        }

		private readonly GeneralMapInfo oldInfo = new GeneralMapInfo();
		private readonly GeneralMapInfo newInfo = new GeneralMapInfo();

		public ChangeGeneralInfo(string author, string lug, string show, DateTime date, string comment)
		{
			// save old data
			oldInfo.mAuthor = Map.Instance.Author.Clone() as string;
			oldInfo.mLUG = Map.Instance.LUG.Clone() as string;
			oldInfo.mEvent = Map.Instance.Event.Clone() as string;
			oldInfo.mDate = Map.Instance.Date;
			oldInfo.mComment = Map.Instance.Comment.Clone() as string;
			// save new data
			newInfo.mAuthor = author.Clone() as string;
			newInfo.mLUG = lug.Clone() as string;
			newInfo.mEvent = show.Clone() as string;
			newInfo.mDate = date;
			newInfo.mComment = comment.Clone() as string;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionChangeGeneralInfo;
		}

		public override void redo()
		{
			Map.Instance.Author = newInfo.mAuthor;
			Map.Instance.LUG = newInfo.mLUG;
			Map.Instance.Event = newInfo.mEvent;
			Map.Instance.Date = newInfo.mDate;
			Map.Instance.Comment = newInfo.mComment;
			// inform MainForm to update its UI
			MainForm.Instance.updateMapGeneralInfo(false);
		}

		public override void undo()
		{
			Map.Instance.Author = oldInfo.mAuthor;
			Map.Instance.LUG = oldInfo.mLUG;
			Map.Instance.Event = oldInfo.mEvent;
			Map.Instance.Date = oldInfo.mDate;
			Map.Instance.Comment = oldInfo.mComment;
			// inform MainForm to update its UI
			MainForm.Instance.updateMapGeneralInfo(false);
		}

		public override bool Equals(object obj)
		{
            // we only change the new data, to know if something new will be changed
            if (obj is ChangeGeneralInfo other)
                return other.newInfo.Equals(this.newInfo);
            // if the specified action is not of the same type as me, for sure it is different
            return false;
		}

        public override int GetHashCode()
        {
            int hashCode = -1652883639;
            hashCode = hashCode * -1521134295 + mUpdateMapView.GetHashCode();
            hashCode = hashCode * -1521134295 + mUpdateLayerView.GetHashCode();
            hashCode = hashCode * -1521134295 + UpdateMapView.GetHashCode();
            hashCode = hashCode * -1521134295 + UpdateLayerView.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<GeneralMapInfo>.Default.GetHashCode(oldInfo);
            hashCode = hashCode * -1521134295 + EqualityComparer<GeneralMapInfo>.Default.GetHashCode(newInfo);
            return hashCode;
        }
    }
}
