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
using System.Drawing;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Maps
{
	class ChangeMapAppearance : Action
	{
		private class LayerChange
		{
			public Layer mReference = null;
			public Layer mOldData = null;
			public Layer mNewData = null;
			public Dictionary<int, Dictionary<int, SolidBrush>> mOldColorMap = null;
		}

		private readonly Color mOldBackGroundColor = Color.Empty;
		private readonly Color mNewBackGroundColor = Color.Empty;
		private readonly List<LayerChange> mLayerChanges = new List<LayerChange>();

		public ChangeMapAppearance(bool isColorModified, bool isFontModified, bool isSizeModified, bool doesAreaChanged)
		{
			// background color of the map
			mOldBackGroundColor = Map.Instance.BackgroundColor;
			mNewBackGroundColor = Properties.Settings.Default.DefaultBackgroundColor;

			// and the other modification to the layer
			bool doesGridChanged = isColorModified || isFontModified || isSizeModified;
			foreach (Layer layer in Map.Instance.LayerList)
			{
				if (doesGridChanged)
				{
                    if (layer is LayerGrid gridLayer)
                    {
                        // create a copy of the edited layer to hold the old data
                        LayerGrid oldLayerData = new LayerGrid();
                        oldLayerData.CopyOptionsFrom(gridLayer);
                        // create a new layer to store the new data
                        LayerGrid newLayerData = new LayerGrid();
                        newLayerData.CopyOptionsFrom(gridLayer);
                        // and change only the grid colors
                        if (isColorModified)
                        {
                            newLayerData.GridColor = Properties.Settings.Default.DefaultGridColor;
                            newLayerData.SubGridColor = Properties.Settings.Default.DefaultSubGridColor;
                        }
                        if (isFontModified)
                        {
                            newLayerData.CellIndexColor = Properties.Settings.Default.DefaultTextColor;
                            newLayerData.CellIndexFont = Properties.Settings.Default.DefaultTextFont;
                        }
                        if (isSizeModified)
                        {
                            newLayerData.GridSizeInStud = Properties.Settings.Default.DefaultGridSize;
                            newLayerData.SubDivisionNumber = Properties.Settings.Default.DefaultSubDivisionNumber;
                            newLayerData.DisplayGrid = Properties.Settings.Default.DefaultGridEnabled;
                            newLayerData.DisplaySubGrid = Properties.Settings.Default.DefaultSubGridEnabled;
                        }

                        // create a new entry for the list and store it in the list
                        LayerChange layerChange = new LayerChange
                        {
                            mReference = gridLayer,
                            mOldData = oldLayerData,
                            mNewData = newLayerData
                        };
                        mLayerChanges.Add(layerChange);
                    }
                }
				if (doesAreaChanged)
				{
                    if (layer is LayerArea areaLayer)
                    {
                        // create a copy of the edited layer to hold the old data
                        LayerArea oldLayerData = new LayerArea();
                        oldLayerData.CopyOptionsFrom(areaLayer);
                        // create a new layer to store the new data
                        LayerArea newLayerData = new LayerArea();
                        newLayerData.CopyOptionsFrom(areaLayer);
                        // and change the area parameters
                        newLayerData.Transparency = Properties.Settings.Default.DefaultAreaTransparency;
                        newLayerData.AreaCellSizeInStud = Properties.Settings.Default.DefaultAreaSize;

                        // create a new entry for the list and store it in the list
                        LayerChange layerChange = new LayerChange
                        {
                            mReference = areaLayer,
                            mOldData = oldLayerData,
                            mNewData = newLayerData,
                            mOldColorMap = areaLayer.ColorMap
                        };
                        mLayerChanges.Add(layerChange);
                    }
                }
			}
		}

		public override string GetName()
		{
			return Properties.Resources.ActionChangeMapAppearance;
		}

		public override void Redo()
		{
			// change the background
			Map.Instance.BackgroundColor = mNewBackGroundColor;
			// change the layers
			foreach (LayerChange layerChange in mLayerChanges)
			{
                // if the layer is an area layer, rescale the colormap
                if (layerChange.mReference is LayerArea areaLayer)
                {
                    if (layerChange.mNewData is LayerArea newlayerArea)
                        areaLayer.rescaleColorMap(newlayerArea.AreaCellSizeInStud);
                }

                // copy the options
                layerChange.mReference.CopyOptionsFrom(layerChange.mNewData);
			}
			// notify the main form
			MainForm.Instance.NotifyForMapBackgroundColorChanged();
		}

		public override void Undo()
		{
			// change the background
			Map.Instance.BackgroundColor = mOldBackGroundColor;
			// change the layers
			foreach (LayerChange layerChange in mLayerChanges)
			{
                // if the layer is an area layer, restore the colormap
                if ((layerChange.mReference is LayerArea areaLayer) && (layerChange.mOldColorMap != null))
                    areaLayer.ColorMap = layerChange.mOldColorMap;

                // copy the options
                layerChange.mReference.CopyOptionsFrom(layerChange.mOldData);
			}
			// notify the main form
			MainForm.Instance.NotifyForMapBackgroundColorChanged();
		}
	}
}
