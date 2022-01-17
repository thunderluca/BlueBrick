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
using System.Drawing;

namespace BlueBrick.Actions.Layers
{
	class ChangeLayerOption : Action
	{
		private readonly Layer mLayer = null;
		private readonly Layer mOldLayerData = null;
		private readonly Layer mNewLayerData = null;
		private readonly bool mLayerNameChanged = false;
		private readonly bool mLayerVisibilityChanged = false;
		private readonly Dictionary<int, Dictionary<int, SolidBrush>> mOldColorMap = null;

		public ChangeLayerOption(Layer layer, Layer oldLayerTemplate, Layer newLayerTemplate)
		{
			mUpdateLayerView = UpdateViewType.LIGHT;
			mUpdateMapView = UpdateViewType.FULL;
			// save the reference of the layer
			mLayer = layer;
			// and create two new layers to save the data in it
			mOldLayerData = oldLayerTemplate;
			mNewLayerData = newLayerTemplate;
			// check if the name changed
			mLayerNameChanged = !oldLayerTemplate.Name.Equals(mNewLayerData.Name);
			// check if visibility changed
			mLayerVisibilityChanged = oldLayerTemplate.Visible != newLayerTemplate.Visible;
            // if the layer is an area layer, save the current color map
            if (layer is LayerArea layerArea)
                mOldColorMap = layerArea.ColorMap;
        }

		public override string GetName()
		{
			string actionName = Properties.Resources.ChangeLayerOption;
			actionName = actionName.Replace("&", mLayer.Name);
			return actionName;
		}

		public override void Redo()
		{
            // if the layer is an area layer, rescale the colormap
            if (mLayer is LayerArea layerArea)
            {
                if (mNewLayerData is LayerArea newlayerArea)
                    layerArea.rescaleColorMap(newlayerArea.AreaCellSizeInStud);
            }

            // copy the options
            mLayer.CopyOptionsFrom(mNewLayerData);

			// notify the main form if the visibility changed
			if (mLayerVisibilityChanged)
				MainForm.Instance.NotifyForLayerVisibilityChangedOrLayerDeletion();

			// notify the part list if the name changed
			if (mLayerNameChanged)
				MainForm.Instance.NotifyPartListForLayerRenamed(mLayer);
		}

		public override void Undo()
		{
            // if the layer is an area layer, restore the colormap
            if ((mLayer is LayerArea layerArea) && (mOldColorMap != null))
                layerArea.ColorMap = mOldColorMap;

            // copy the options
            mLayer.CopyOptionsFrom(mOldLayerData);

			// notify the main form if the visibility changed
			if (mLayerVisibilityChanged)
				MainForm.Instance.NotifyForLayerVisibilityChangedOrLayerDeletion();

			// notify the part list if the name changed
			if (mLayerNameChanged)
				MainForm.Instance.NotifyPartListForLayerRenamed(mLayer);
		}
	}
}
