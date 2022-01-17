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
using System.Drawing.Drawing2D;
using BlueBrick.MapData;
using BlueBrick.Actions.Items;

namespace BlueBrick.Actions.Texts
{
	class RotateText : RotateItems
	{
		public RotateText(LayerText layer, List<Layer.LayerItem> texts, int rotateSteps)
			: this(layer, texts, rotateSteps, false)
		{
		}

		public RotateText(LayerText layer, List<Layer.LayerItem> texts, int rotateSteps, bool forceKeepLastCenter)
		{
			// call the common constructor
			float angle = Layer.CurrentRotationStep * rotateSteps;
			base.commonConstructor(layer, texts, angle, forceKeepLastCenter);
		}

		public override string GetName()
		{
			if (mItems.Count == 1)
			{
				string actionName = Properties.Resources.ActionRotateText;
				string text = (mItems[0] as LayerText.TextCell).Text.Replace("\r\n", " ");
				if (text.Length > 10)
					text = text.Substring(0, 10) + "...";
				actionName = actionName.Replace("&", text);
				return actionName;
			}
			else
			{
				return Properties.Resources.ActionRotateSeveralTexts;
			}
		}

		public override void Redo()
		{
			// get the rotation angle according to the rotation direction
			float rotationAngle = mRotateCW ? -mRotationStep : mRotationStep;

			// rotate all the objects
			Matrix rotation = new Matrix();
			rotation.Rotate(rotationAngle);
			foreach (Layer.LayerItem item in mItems)
				rotate(item, rotation, rotationAngle, true);

			// rotate also the groups in order to rotate their snap margin and to adjust their display area
			rotateGroups(rotationAngle);

			// update the bounding rectangle (because the text is not square)
			mLayer.UpdateBoundingSelectionRectangle();
		}

		public override void Undo()
		{
			// get the rotation angle according to the rotation direction
			float rotationAngle = mRotateCW ? mRotationStep : -mRotationStep;

			// rotate all the objects
			Matrix rotation = new Matrix();
			rotation.Rotate(rotationAngle);
			foreach (Layer.LayerItem item in mItems)
				rotate(item, rotation, rotationAngle, true);

			// rotate also the groups in order to rotate their snap margin and to adjust their display area
			rotateGroups(rotationAngle);

			// update the bounding rectangle (because the text is not square)
			mLayer.UpdateBoundingSelectionRectangle();
		}
	}
}
