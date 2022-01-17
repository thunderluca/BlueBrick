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
using System.Text;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Rulers
{
	class DeleteRuler : Action
	{
		private readonly LayerRuler mRulerLayer = null;
		private readonly List<Layer.LayerItem> mRulers = null;
		private readonly List<int> mRulerIndex = null; // this list of index is for the redo, to add each ruler at the same place

		public DeleteRuler(LayerRuler layer, List<Layer.LayerItem> rulers)
		{
			mRulerLayer = layer;
			mRulerIndex = new List<int>(rulers.Count);
			// copy the list, because the pointer may change (specially if it is the selection)
			mRulers = new List<Layer.LayerItem>(rulers.Count);
			foreach (Layer.LayerItem obj in rulers)
				mRulers.Add(obj);
		}

		public override string GetName()
		{
			if (mRulers.Count == 1)
				return Properties.Resources.ActionDeleteRuler;
			else
				return Properties.Resources.ActionDeleteSeveralRulers;
		}

		public override void Redo()
		{
			// remove the specified rulers from the list of the layer,
			// but do not delete it, also memorise its last position
			mRulerIndex.Clear();
			foreach (Layer.LayerItem obj in mRulers)
				mRulerIndex.Add(mRulerLayer.RemoveRulerItem(obj as LayerRuler.RulerItem));
		}

		public override void Undo()
		{
			// and add all the texts in the reverse order
			for (int i = mRulers.Count - 1; i >= 0; --i)
				mRulerLayer.AddRulerItem(mRulers[i] as LayerRuler.RulerItem, mRulerIndex[i]);
		}
	}
}
