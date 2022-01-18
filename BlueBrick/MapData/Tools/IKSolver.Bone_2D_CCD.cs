/******************************************************************************
  Copyright (c) 2008-2009 Ryan Juckett
  http://www.ryanjuckett.com/

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.

  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.

  3. This notice may not be removed or altered from any source
     distribution.
******************************************************************************/
/******************************************************************************
 * 
 * This code was Modified by Alban Nanty to suit the needs of BlueBricks.
 * The IK Solver is used to solve the position of the PFS Flex Tracks.
 * 
 *****************************************************************************/

using System;

namespace BlueBrick.MapData.Tools
{
    partial class IKSolver
    {
        ///***************************************************************************************
        /// Bone_2D_CCD
        /// This class is used to supply the CalcIK_2D_CCD function with a bone's representation
        /// relative to its parent in the kinematic chain.
        /// author: Ryan Juckett
        ///***************************************************************************************
        public class Bone_2D_CCD
        {
            public double localAngleInRad = 0.0f; // angle in parent space
            public double maxAbsoluteAngleInRad = Math.PI * 2; // the maximum angle that can take this bone
            public double worldX = 0.0f;        // x position in world space
            public double worldY = 0.0f;        // y position in world space
            public LayerBrick.Brick.ConnectionPoint connectionPoint = null; // the connection Point related to this bone
        };
    }
}
