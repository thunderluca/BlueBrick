﻿/******************************************************************************
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
using System.Text;
using System.Diagnostics;

namespace BlueBrick.MapData.FlexTrack
{
	class IKSolver
	{
		///***************************************************************************************
		/// SimplifyAngle
		/// This function will convert an angle to the equivalent rotation in the range [-pi,pi]
		/// author: Ryan Juckett
		///***************************************************************************************
		private static double SimplifyAngle(double angle)
		{
			double twoPi = (2.0 * Math.PI);
			angle = angle % twoPi;
			if( angle < -Math.PI )
				angle += twoPi;
			else if( angle > Math.PI )
				angle -= twoPi;
			return angle;
		}

		///***************************************************************************************
		/// Bone_2D_CCD
		/// This class is used to supply the CalcIK_2D_CCD function with a bone's representation
		/// relative to its parent in the kinematic chain.
		/// author: Ryan Juckett
		///***************************************************************************************
		public class Bone_2D_CCD
		{
			public double localX;     // x position in parent space
			public double localY;     // y position in parent space
			public double localAngleInRad; // angle in parent space
			public double worldX;        // x position in world space
			public double worldY;        // y position in world space
			public double worldAngle;    // angle in world space
			public double worldCosAngle; // sine of angle
			public double worldSinAngle; // cosine of angle
			public LayerBrick.Brick.ConnectionPoint connectionPoint; // the connection Point related to this bone
		};

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
			double arrivalDist           // Must get within this range of the target
		)
		{
			// Set an epsilon value to prevent division by small numbers.
			const double epsilon = 0.0001; 

			// Set max arc length a bone can move the end effector and be considered no motion
			// so that we can detect a failure state.
			const double trivialArcLength = 0.00001; 


			int numBones = bones.Count;
			if (numBones == 0)
				return CCDResult.Failure;

			double arrivalDistSqr = arrivalDist * arrivalDist;

			//===
			// Compute the world space bone data.

			// Start with the root bone.
			//bones[0].worldX = bones[0].localX;
			//bones[0].worldY = bones[0].localY;
			//bones[0].worldAngle = bones[0].localAngle;
			//bones[0].worldCosAngle = Math.Cos(bones[0].worldAngle);
			//bones[0].worldSinAngle = Math.Sin(bones[0].worldAngle);
			
			//// Convert child bones to world space.
			//for( int boneIdx = 1; boneIdx < numBones; ++boneIdx )
			//{
			//    Bone_2D_CCD prevWorldBone = bones[boneIdx - 1];
			//    Bone_2D_CCD curLocalBone = bones[boneIdx];

			//    curLocalBone.worldX = prevWorldBone.worldX + prevWorldBone.worldCosAngle * curLocalBone.localX
			//                                     - prevWorldBone.worldSinAngle*curLocalBone.localY;
			//    curLocalBone.worldY = prevWorldBone.worldY + prevWorldBone.worldSinAngle * curLocalBone.localX
			//                                     + prevWorldBone.worldCosAngle*curLocalBone.localY;
			//    curLocalBone.worldAngle = prevWorldBone.worldAngle + curLocalBone.localAngle;
			//    curLocalBone.worldCosAngle = Math.Cos(curLocalBone.worldAngle);
			//    curLocalBone.worldSinAngle = Math.Sin(curLocalBone.worldAngle);
			//}
			
			//===
			// Track the end effector position (the final bone)
			double endX = bones[numBones - 1].worldX;
			double endY = bones[numBones - 1].worldY;

			//===
			// Perform CCD on the bones by optimizing each bone in a loop 
			// from the final bone to the root bone
			bool modifiedBones = false;
			for( int boneIdx = numBones-2; boneIdx >= 0; --boneIdx )
			{
				// Get the vector from the current bone to the end effector position.
				double curToEndX = endX - bones[boneIdx].worldX;
				double curToEndY = endY - bones[boneIdx].worldY;
				double curToEndMag = Math.Sqrt((curToEndX * curToEndX) + (curToEndY * curToEndY));

				// Get the vector from the current bone to the target position.
				double curToTargetX = targetX - bones[boneIdx].worldX;
				double curToTargetY = targetY - bones[boneIdx].worldY;
				double curToTargetMag = Math.Sqrt((curToTargetX * curToTargetX) + (curToTargetY * curToTargetY));

				// Get rotation to place the end effector on the line from the current
				// joint position to the target postion.
				double cosRotAng;
				double sinRotAng;
				double endTargetMag = (curToEndMag*curToTargetMag);
				if( endTargetMag <= epsilon )
				{
					cosRotAng = 1;
					sinRotAng = 0;
				}
				else
				{
					cosRotAng = (curToEndX*curToTargetX + curToEndY*curToTargetY) / endTargetMag;
					sinRotAng = (curToEndX*curToTargetY - curToEndY*curToTargetX) / endTargetMag;
				}

				// Clamp the cosine into range when computing the angle (might be out of range
				// due to floating point error).
				double rotAng = Math.Acos( Math.Max(-1, Math.Min(1,cosRotAng) ) );
				if( sinRotAng < 0.0 )
					rotAng = -rotAng;
				
				// Rotate the end effector position.
				endX = bones[boneIdx].worldX + cosRotAng * curToEndX - sinRotAng * curToEndY;
				endY = bones[boneIdx].worldY + sinRotAng * curToEndX + cosRotAng * curToEndY;

				// Rotate the current bone in local space (this value is output to the user)
				bones[boneIdx].localAngleInRad = SimplifyAngle( bones[boneIdx].localAngleInRad + rotAng );

				// Check for termination
				double endToTargetX = (targetX-endX);
				double endToTargetY = (targetY-endY);
				if( endToTargetX*endToTargetX + endToTargetY*endToTargetY <= arrivalDistSqr )
				{
					// We found a valid solution.
					return CCDResult.Success;
				}

				// Track if the arc length that we moved the end effector was
				// a nontrivial distance.
				if( !modifiedBones && Math.Abs(rotAng)*curToEndMag > trivialArcLength )
				{
					modifiedBones = true;
				}
			}

			// We failed to find a valid solution during this iteration.
			if( modifiedBones )
				return CCDResult.Processing;
			else
				return CCDResult.Failure;
		}	
	}
}
