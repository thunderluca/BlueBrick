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
using System.Drawing;

namespace BlueBrick.MapData.Tools
{
    /// <summary>
    /// This a node in the graph of all the possible paths.
    /// </summary>
    public class AStarNode
	{
		/// <summary>
		/// The brick to which this node refer.
		/// </summary>
		public LayerBrick.Brick mBrick = null;

		/// <summary>
		/// The parent node in the path. Used to come back to the start when we reached the goal.
		/// </summary>
		public AStarNode mParentNode = null;

		/// <summary>
		/// The cost of the current path from the start node to this one.
		/// Sorry the name of this variable is not explicit but refer to its classical name
		/// in the A* documentations. Please see an A* doc for more details.
		/// </summary>
		public float g = 0.0f;

		/// <summary>
		/// A heuristic cost of the path from this node to the goal node.
		/// Sorry the name of this variable is not explicit but refer to its classical name
		/// in the A* documentations. Please see an A* doc for more details.
		/// </summary>
		public float h = 0.0f;

		/// <summary>
		/// The global cost of the path if the path use this node.
		/// Sorry the name of this variable is not explicit but refer to its classical name
		/// in the A* documentations. Please see an A* doc for more details.
		/// </summary>
		public float f = 0.0f;

		/// <summary>
		/// construct a new node
		/// </summary>
		/// <param name="brick">the brick corresponding to this node</param>
		public AStarNode(LayerBrick.Brick brick)
		{
			mBrick = brick;
		}

		/// <summary>
		/// Compute the parameter g, h, f of this node.
		/// </summary>
		public void ComputeParameters(LayerBrick.Brick goalBrick)
		{
			var distance = new PointF();

			// we will compute g first
			if (mParentNode != null)
			{
				// the cost between this node and the previous one is always equal to the distance between
				// this brick and the parent brick position
				PointF parentCenter = mParentNode.mBrick.Center;
				distance.X = parentCenter.X - mBrick.Center.X;
				distance.Y = parentCenter.Y - mBrick.Center.Y;

				// g is the cost of the path from start node to this node. This is a real cost, defined by the algorithm, can't be changed.
				// so g equals to the cost from the start node to the previous one PLUS the cost between the previous one and this one.
				g = mParentNode.g + (float)Math.Sqrt((distance.X*distance.X) + (distance.Y*distance.Y));
			}
			else
			{
				g = 0;	// no parent that means this node is the start node, so g equals to 0
			}

			// h is the cost of the path from this node to the goal node. This is not a real cost,
			// just an heuristic, that is to say the remaining distance.
			distance.X = goalBrick.Center.X - mBrick.Center.X;
			distance.Y = goalBrick.Center.Y - mBrick.Center.Y;
			h = (float)Math.Sqrt((distance.X * distance.X) + (distance.Y * distance.Y));

			// f is the global cost. Defined by the algorithm, can't be changed.
			f = g + h;
		}
	}
}
