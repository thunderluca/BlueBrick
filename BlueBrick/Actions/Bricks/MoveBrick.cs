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
using BlueBrick.Actions.Items;

namespace BlueBrick.Actions.Bricks
{
	public class MoveBrick : MoveItems
	{
		private readonly string mPartNumber = string.Empty; //if the list contains only one brick or one group, this is the name of this specific brick or group

		public MoveBrick(LayerBrick layer, List<Layer.LayerItem> bricks, PointF move)
			: base(layer, bricks, move)
		{
			// try to get a part number (which can be the name of a group)
			Layer.LayerItem topItem = Layer.SGetTopItemFromList(mItems);
			if (topItem != null)
				mPartNumber = topItem.PartNumber; // part number is virtual, works both for part and group
		}

		public override string GetName()
		{
			// if the part number is valid, use the specific message
			if (mPartNumber != string.Empty)
			{
				string actionName = Properties.Resources.ActionMoveBrick;
				actionName = actionName.Replace("&", mPartNumber);
				return actionName;
			}
			else
			{
				return Properties.Resources.ActionMoveSeveralBricks;
			}
		}

		public override void Redo()
		{
			LayerBrick brickLayer = mLayer as LayerBrick;
			// reselect the items of the action, cause after we will update the connectivity of the selection
			// and do it before calling the base class cause the base class will update the selection rectangle
			brickLayer.SelectOnlyThisObject(mItems);
			// call the base class
			base.Redo();
			// update the brick connectivity
			brickLayer.updateBrickConnectivityOfSelection(false);
			// notify the main form for the brick move
			MainForm.Instance.NotifyForPartMoved();
		}

		public override void Undo()
		{
			LayerBrick brickLayer = mLayer as LayerBrick;
			// reselect the items of the action, cause after we will update the connectivity of the selection
			// and do it before calling the base class cause the base class will update the selection rectangle
			brickLayer.SelectOnlyThisObject(mItems);
			// call the base class
			base.Undo();
			// update the brick connectivity
			brickLayer.updateBrickConnectivityOfSelection(false);
			// notify the main form for the brick move
			MainForm.Instance.NotifyForPartMoved();
		}
	}
}
