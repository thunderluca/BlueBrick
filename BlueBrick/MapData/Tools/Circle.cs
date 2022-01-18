using System;
using System.Drawing;

namespace BlueBrick.MapData.Tools
{
    /// <summary>
    /// This class represent a circle area
    /// </summary>
    public class Circle : Surface
	{
		private PointF mCenter = new PointF();

		#region get / set
		public override PointF this[int i]
		{
			get
			{
				// we don't care of the index, always return the center
				return mCenter;
			}
			set
			{
				// we don't care of the index, always set the center
				mCenter = value;
			}
		}

		/// <summary>
		/// get or set an array of vertices describing this surface
		/// </summary>
		public override PointF[] Vertice
		{
			get { return new PointF[] { mCenter }; }
			set { mCenter = value[0]; }
		}

		/// <summary>
		/// get or set the center of the circle
		/// </summary>
		public PointF Center
        {
            get { return mCenter; }
            set { mCenter = value; }
        }

		/// <summary>
		/// get or set the radius of the circle
		/// </summary>
		public float Radius { get; set; }
		#endregion
		
		#region constructor
		public Circle(PointF center, float radius)
		{
			mCenter = center;
			Radius = radius;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="model">the model from which copy</param>
		public Circle(Circle model)
		{
			mCenter = model.Center;
			Radius = model.Radius;
		}

		/// <summary>
		/// Create a copy instance of this instance
		/// </summary>
		/// <returns>a conform copy of this instance of Surface</returns>
		public override Surface Clone()
		{
			// call the copy constructor
			return new Circle(this);
		}
		#endregion

		#region function
		/// <summary>
		/// Tells if the given point is inside this circle, assuming the point and the surface are
		/// in the same coordinate system.
		/// </summary>
		/// <param name="point">the point coordinate to test</param>
		/// <returns>true if the point is inside this circle</returns>
		public override bool IsPointInside(PointF point)
		{
			// compute the distance between the point and the center
			var dx = mCenter.X - point.X;
			var dy = mCenter.Y - point.Y;
			// true if the distance is lower than the radius
			var distance = (float)Math.Sqrt((dx * dx) + (dy * dy));
			return distance < Radius;
		}

		/// <summary>
		/// Tells if the specified axis aligned rectangle is intersecting or overlapping this surface,
		/// assuming the rectangle and the surface are in the same coordinate system.
		/// </summary>
		/// <param name="rectangle">the rectangle to test</param>
		/// <returns>true if the rectangle intersects or overlaps this surface</returns>
		public override bool IsRectangleIntersect(RectangleF rectangle)
		{
			// check the angles first
			var isOnLeft = mCenter.X < rectangle.Left;
			var isOnRight = mCenter.X > rectangle.Right;
			var isOnTop = mCenter.Y < rectangle.Top;
			var isOnBottom = mCenter.Y > rectangle.Bottom;

			if (isOnLeft && isOnTop)
			{
				return IsPointInside(new PointF(rectangle.Left, rectangle.Top));
			}
			else if (isOnRight && isOnTop)
			{
				return IsPointInside(new PointF(rectangle.Right, rectangle.Top));
			}
			else if (isOnRight && isOnBottom)
			{
				return IsPointInside(new PointF(rectangle.Right, rectangle.Bottom));
			}
			else if (isOnLeft && isOnBottom)
			{
				return IsPointInside(new PointF(rectangle.Left, rectangle.Bottom)) ;
			}
			else
			{
				// compute an bigger rectangle increased on all border by the radius of the circle
				var diameter = Radius * 2.0f;
				var bigRectangle = new RectangleF(
					x: rectangle.X - Radius,
					y: rectangle.Y - Radius,
					width: rectangle.Width + diameter,
					height: rectangle.Height + diameter
				);
				// then check if the center is inside the big rectangle
				return (mCenter.X > bigRectangle.Left) && (mCenter.X < bigRectangle.Right) &&
						(mCenter.Y > bigRectangle.Top) && (mCenter.Y < bigRectangle.Bottom);
			}
		}

		/// <summary>
		/// Translate this surface from the given vector
		/// </summary>
		/// <param name="translationVector">a vector describing the translation</param>
		public override void Translate(PointF translationVector)
		{
			mCenter.X += translationVector.X;
			mCenter.Y += translationVector.Y;
		}
		#endregion
	}
}
