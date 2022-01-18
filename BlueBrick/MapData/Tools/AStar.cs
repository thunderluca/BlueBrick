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

namespace BlueBrick.MapData.Tools
{
    public class AStar
	{
		private static readonly AStarNodeList sOpenList = new AStarNodeList();
		private static readonly AStarNodeList sCloseList = new AStarNodeList();

		public static List<Layer.LayerItem> FindPath(LayerBrick.Brick startBrick, LayerBrick.Brick goalBrick)
		{
			var result = new List<Layer.LayerItem>();

			// init some variables
			sOpenList.Clear();
			sCloseList.Clear();

			// create the first node with the starting brick
			AStarNode currentNode = new AStarNode(startBrick);
			currentNode.ComputeParameters(goalBrick);

			// start of the loop
			while (currentNode != null)
			{
				// get the refernce of the current brick
				LayerBrick.Brick currentBrick = currentNode.mBrick;

				// check if we reached the goal
				if (currentBrick == goalBrick)
				{
					// the goal is reached, put all the bricks in the result list
					for (AStarNode node = currentNode; node != null; node = node.mParentNode)
						result.Add(node.mBrick);
					// reverse the list to have the path from start to goal
					result.Reverse();
					return result;
				}

				// now iterate on all the connexion point of the current brick
				var connexionList = currentBrick.ConnectionPoints;
				if (connexionList != null)
					foreach (LayerBrick.Brick.ConnectionPoint connexion in connexionList)
					{
						// check if the connexion is free, or if there is a brick connected to it
						LayerBrick.Brick neighborBrick = connexion.ConnectedBrick;
						if (neighborBrick == null)
							continue;

                        // we found a valid connexion, create a new node for this new potential brick to explore
                        AStarNode potentialNewNeighborNode = new AStarNode(neighborBrick)
                        {
                            mParentNode = currentNode
                        };
                        potentialNewNeighborNode.ComputeParameters(goalBrick);

						// try to search the neighbor brick in the close list
						AStarNode neighborNode = sCloseList.Find(neighborBrick);
						if (neighborNode != null)
						{
							// we found this brick in the close list, that means this brick was already explored,
							// but we need to check if we found a shorter way, by checking the f values
							// so we check if the node stay where it is
							if (neighborNode.f <= potentialNewNeighborNode.f)
								continue; // that's fine the previous exploration was the shorter way
							// else we found a shorter way
							// so we remove the node from the close list (cause we will add it in open)
							sCloseList.Remove(neighborNode);
						}
						else
						{
							// the neighbor brick is not in the close list so now we check if it is in open list
							neighborNode = sOpenList.Find(neighborBrick);
							if (neighborNode != null)
							{
								// the brick is already in the open list but we check if we have found a shorter way
								// by checking the f values.
								if (neighborNode.f <= potentialNewNeighborNode.f)
									continue; // that's fine the new way we found is not shorter
								// else that mean the new way is shorter
								// so we remove the node from the open list (cause we will add the better one)
								sOpenList.Remove(neighborNode);
							}
						}

						// If we reach this point, that means the potential new node is valid and
						// must be added in the open list
						sOpenList.Add(potentialNewNeighborNode);
					}

				// the current node is finished to be expored, so add it in the close list
				sCloseList.Add(currentNode);

				// get the next node to explore
				if (sOpenList.Count > 0)
				{
					currentNode = sOpenList[0];
					sOpenList.RemoveAt(0);
				}
				else
				{
					currentNode = null;
				}
			}

			// the open list is empty and we didn't find the goal brick, so the search failed.
			return result;
		}
	}
}
