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

namespace BlueBrick.MapData.Tools
{
    /// <summary>
    /// This class encaplusate a float value representing a distance.
    /// You can then ask the distance value in different unit: stud, ldu, module, meter or feet
    /// </summary>
    public partial class Distance
    {
        #region get/set
        /// <summary>
        /// get or set the current unit for this distance
        /// </summary>
        public Unit CurrentUnit { get; set; }

        /// <summary>
        /// get or set the distance in the current unit
        /// </summary>
        public float DistanceInCurrentUnit
        {
            get
            {
                switch (CurrentUnit)
                {
                    case Unit.STUD: return DistanceInStud;
                    case Unit.LDU: return DistanceInLDU;
                    case Unit.STRAIGHT_TRACK: return DistanceInStraightTrack;
                    case Unit.MODULE: return DistanceInModule;
                    case Unit.METER: return DistanceInMeter;
                    case Unit.FEET: return DistanceInFeet;
                }
                return DistanceInStud;
            }

            set
            {
                switch (CurrentUnit)
                {
                    case Unit.STUD: DistanceInStud = value; break;
                    case Unit.LDU: DistanceInLDU = value; break;
                    case Unit.STRAIGHT_TRACK: DistanceInStraightTrack = value; break;
                    case Unit.MODULE: DistanceInModule = value; break;
                    case Unit.METER: DistanceInMeter = value; break;
                    case Unit.FEET: DistanceInFeet = value; break;
                }
            }
        }

        /// <summary>
        /// get or set the distance in stud
        /// </summary>
        public float DistanceInStud { get; set; }

        /// <summary>
        /// Get or set the distance in LDraw Unit (LDU), knowing that 1 Stud = 20 LDU.
        /// </summary>
        public float DistanceInLDU
        {
            get { return DistanceInStud * 20.0f; }
            set { DistanceInStud = value * 0.05f; }
        }

        /// <summary>
        /// Get or set the distance in AFOL Module, knowing that 1 Module = 96 studs.
        /// </summary>
        public float DistanceInModule
        {
            get { return DistanceInStud / 96.0f; }
            set { DistanceInStud = value * 96.0f; }
        }

        /// <summary>
        /// Get or set the distance in meter, knowing that 1 Stud = 8 mm.
        /// </summary>
        public float DistanceInMeter
        {
            get { return DistanceInStud * 0.008f; }
            set { DistanceInStud = value * 125.0f; }
        }

        /// <summary>
        /// Get or set the distance in feet, knowing that 1 Stud = 8 mm.
        /// </summary>
        public float DistanceInFeet
        {
            get { return DistanceInStud * 0.026248f; }
            set { DistanceInStud = value * 38.09814081f; }
        }

        /// <summary>
        /// Get or set the distance rail straight track, knowing that 1 track is 16 studs longs.
        /// </summary>
        public float DistanceInStraightTrack
        {
            get { return DistanceInStud * 0.0625f; }
            set { DistanceInStud = value * 16.0f; }
        }
        #endregion

        #region constructor
        public Distance()
        {
        }

        /// <summary>
        /// Construct a distance instance and initialized it with the specified distance in the specified unit.
        /// The current unit will be set with the specified unit.
        /// </summary>
        /// <param name="distance">the initial value in stud unit</param>
        /// <param name="unit">the unit of the initial value</param>
        public Distance(float distance, Unit unit)
        {
            // first set the unit
            CurrentUnit = unit;
            // then set the value
            DistanceInCurrentUnit = distance;
        }
        #endregion

        #region tools
        /// <summary>
        /// Get the string representing the unit in the current language of the application.
        /// For example in english it will be "stud", "LDU", "Mod", "m" and "ft"
        /// </summary>
        /// <returns>A string representing the current unit</returns>
        public string GetCurrentUnitName()
        {
            switch (CurrentUnit)
            {
                case Unit.STUD:
                    return Properties.Resources.UnitStud;
                case Unit.LDU:
                    return Properties.Resources.UnitLDU;
                case Unit.STRAIGHT_TRACK:
                    return Properties.Resources.UnitStraightTrack;
                case Unit.MODULE:
                    return Properties.Resources.UnitModule;
                case Unit.METER:
                    return Properties.Resources.UnitMeter;
                case Unit.FEET:
                    return Properties.Resources.UnitFeet;
            }

            return string.Empty;
        }

        /// <summary>
        /// Return a string representing the distance in its current unit using the specified format
        /// and followed by the unit if the boolean flag is set
        /// </summary>
        /// <param name="format">A string format that will be used to convert the number into a string</param>
        /// <param name="withUnit">if true, a space and the unit name (in the current language of the application) is added at the end</param>
        /// <returns></returns>
        public string ToString(string format, bool withUnit)
        {
            // convert the value into string using the format
            var formatedDistance = DistanceInCurrentUnit.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            
            // add the unit name if needed
            if (withUnit)
            {
                formatedDistance += " " + GetCurrentUnitName();
            }
            
            // return the constructed string
            return formatedDistance;
        }
        #endregion
    }
}
