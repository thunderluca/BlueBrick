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

namespace BlueBrick.Actions
{
	public class HideLayer : Action
	{
		private readonly Layer mLayer = null;

		public HideLayer(Layer layer)
		{
			mUpdateLayerView = UpdateViewType.LIGHT;
			mLayer = layer;
		}

		public override string GetName()
		{
			string actionName = Properties.Resources.ActionHideLayer;
			actionName = actionName.Replace("&", mLayer.Name);
			return actionName;
		}

		public override void Redo()
		{
			Map.Instance.HideLayer(mLayer);

			// notify the main form for layer visibility change
			MainForm.Instance.NotifyForLayerVisibilityChangedOrLayerDeletion();
		}

		public override void Undo()
		{
			Map.Instance.ShowLayer(mLayer);

			// notify the main form for layer visibility change
			MainForm.Instance.NotifyForLayerVisibilityChangedOrLayerDeletion();
		}
	}
}
