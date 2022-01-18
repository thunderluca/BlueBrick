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

namespace BlueBrick.MapData.Tools
{
    partial class IKSolver
    {
        ///***************************************************************************************
        /// CCDResult
        /// This enum represents the resulting state of a CCD iteration.
        /// author: Ryan Juckett
        ///***************************************************************************************
        public enum CCDResult
        {
            Success,    // the target was reached
            Processing, // still trying to reach the target
            Failure,    // failed to reach the target
        }
    }
}
