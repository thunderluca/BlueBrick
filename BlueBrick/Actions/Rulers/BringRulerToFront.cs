﻿// BlueBrick, a LEGO(c) layout editor.
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
using BlueBrick.Actions.Items;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Rulers
{
	class BringRulerToFront : ChangeItemOrder
	{
		public BringRulerToFront(Layer layer, List<Layer.LayerItem> rulersToMove)
			: base(layer, rulersToMove, FrontOrBack.FRONT)
		{
		}

		public override string GetName()
		{
			return Properties.Resources.ActionBringRulerToFront;
		}

		protected override int removeItem(Layer.LayerItem item)
		{
			return (mLayer as LayerRuler).RemoveRulerItem(item as LayerRuler.RulerItem);
		}

		protected override void addItem(Layer.LayerItem item, int position)
		{
			(mLayer as LayerRuler).AddRulerItem(item as LayerRuler.RulerItem, position);
		}
	}
}
