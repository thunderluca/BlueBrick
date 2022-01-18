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
using System.Collections.Generic;

namespace BlueBrick.MapData.Tools
{
    partial class IKSolver
    {
        ///***************************************************************************************
        /// SimplifyAngle
        /// This function will convert an angle to the equivalent rotation in the range [-pi,pi]
        /// author: Ryan Juckett
        ///***************************************************************************************
        private static double SimplifyAngle(double angle)
        {
            var twoPi = 2.0 * Math.PI;

            var simplifiedAngle = angle % twoPi;

            switch (simplifiedAngle)
            {
                case var a when a < -Math.PI:
                    return simplifiedAngle + twoPi;
                case var a when a > Math.PI:
                    return simplifiedAngle - twoPi;
                default:
                    return simplifiedAngle;
            }
        }

        ///***************************************************************************************
        /// CalcIK_2D_CCD
        /// Given a bone chain located at the origin, this function will perform a single cyclic
        /// coordinate descent (CCD) iteration. This finds a solution of bone angles that places
        /// the final bone in the given chain at a target position. The supplied bone angles are
        /// used to prime the CCD iteration. If a valid solution does not exist, the angles will
        /// move as close to the target as possible. The user should resupply the updated angles 
        /// until a valid solution is found (or until an iteration limit is met).
        ///  
        /// returns: CCDResult.Success when a valid solution was found.
        ///          CCDResult.Processing when still searching for a valid solution.
        ///          CCDResult.Failure when it can get no closer to the target.
        /// author: Ryan Juckett
        ///***************************************************************************************
        public static CCDResult CalcIK_2D_CCD
        (
            ref List<Bone_2D_CCD> bones, // Bone values to update
            double targetX,              // Target x position for the end effector
            double targetY,              // Target y position for the end effector
            double arrivalDist,           // Must get within this range of the target
            int numBones                // the number of bones to use in the bone list
        )
        {
            if (numBones < 2)
            {
                return CCDResult.Failure;
            }

            // Set an epsilon value to prevent division by small numbers.
            const double epsilon = 0.0001;

            // Set max arc length a bone can move the end effector and be considered no motion
            // so that we can detect a failure state.
            const double trivialArcLength = 0.00001;

            var arrivalDistSqr = arrivalDist * arrivalDist;

            //===
            // Track the end effector position (the final bone)
            var endX = bones[numBones - 1].worldX;
            var endY = bones[numBones - 1].worldY;

            //===
            // Perform CCD on the bones by optimizing each bone in a loop 
            // from the final bone to the root bone
            var modifiedBones = false;
            for (int boneIdx = numBones - 2; boneIdx >= 0; --boneIdx)
            {
                // Get the vector from the current bone to the end effector position.
                var curToEndX = endX - bones[boneIdx].worldX;
                var curToEndY = endY - bones[boneIdx].worldY;
                var curToEndMag = Math.Sqrt((curToEndX * curToEndX) + (curToEndY * curToEndY));

                // Get the vector from the current bone to the target position.
                var curToTargetX = targetX - bones[boneIdx].worldX;
                var curToTargetY = targetY - bones[boneIdx].worldY;
                var curToTargetMag = Math.Sqrt((curToTargetX * curToTargetX) + (curToTargetY * curToTargetY));

                // Get rotation to place the end effector on the line from the current
                // joint position to the target postion.
                double cosRotAng;
                double sinRotAng;
                var endTargetMag = curToEndMag * curToTargetMag;
                if (endTargetMag <= epsilon)
                {
                    cosRotAng = 1;
                    sinRotAng = 0;
                }
                else
                {
                    cosRotAng = (curToEndX * curToTargetX + curToEndY * curToTargetY) / endTargetMag;
                    sinRotAng = (curToEndX * curToTargetY - curToEndY * curToTargetX) / endTargetMag;
                }

                // Clamp the cosine into range when computing the angle (might be out of range
                // due to floating point error).
                var rotAng = Math.Acos(Math.Max(-1, Math.Min(1, cosRotAng))) * (sinRotAng < 0.0 ? -1 : 1);

                // Rotate the current bone in local space (this value is output to the user)
                // and clamp the value inside the limit defined in the bone
                var newLocalAngleInRad = SimplifyAngle(bones[boneIdx].localAngleInRad + rotAng);
                var maxLocalAngleInRad = bones[boneIdx].maxAbsoluteAngleInRad;
                var needToRecomputeCosAndSin = false;
                if (newLocalAngleInRad > maxLocalAngleInRad)
                {
                    rotAng -= newLocalAngleInRad - maxLocalAngleInRad;
                    newLocalAngleInRad = maxLocalAngleInRad;
                    needToRecomputeCosAndSin = true;
                }
                else if (newLocalAngleInRad < -maxLocalAngleInRad)
                {
                    rotAng -= newLocalAngleInRad + maxLocalAngleInRad;
                    newLocalAngleInRad = -maxLocalAngleInRad;
                    needToRecomputeCosAndSin = true;
                }

                // Set the angle in the current bone in local space (this value is output to the user)
                bones[boneIdx].localAngleInRad = newLocalAngleInRad;

                // if we clamped the angle, recompute the cos and sin
                if (needToRecomputeCosAndSin)
                {
                    cosRotAng = Math.Cos(rotAng);
                    sinRotAng = Math.Sin(rotAng);
                }

                // Rotate the end effector position.
                endX = bones[boneIdx].worldX + cosRotAng * curToEndX - sinRotAng * curToEndY;
                endY = bones[boneIdx].worldY + sinRotAng * curToEndX + cosRotAng * curToEndY;

                // Check for termination (except if we need to swap the target
                var endToTargetX = targetX - endX;
                var endToTargetY = targetY - endY;
                if (endToTargetX * endToTargetX + endToTargetY * endToTargetY <= arrivalDistSqr)
                {
                    // We found a valid solution.
                    return CCDResult.Success;
                }

                // Track if the arc length that we moved the end effector was
                // a nontrivial distance.
                if (!modifiedBones && Math.Abs(rotAng) * curToEndMag > trivialArcLength)
                {
                    modifiedBones = true;
                }
            }

            // We failed to find a valid solution during this iteration.
            return modifiedBones ? CCDResult.Processing : CCDResult.Failure;
        }
    }
}
