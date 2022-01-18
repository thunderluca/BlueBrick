using System.Drawing;

namespace BlueBrick.MapData.Tools
{
    /// <summary>
    /// This is a base and abstract class for an area and
    /// that can be implemented by a circle, a rectangle or a polygon
    /// </summary>
    public abstract class Surface
	{
		#region get / set
		/// <summary>
		/// get/set the geometrical point describing this surface for the specified index
		/// </summary>
		/// <param name="index">the index defining which point you want</param>
		/// <returns>the indexed point</returns>
		public abstract PointF this[int i]
		{
			get;
			set;
		}

		/// <summary>
		/// get or set an array of vertices describing this surface
		/// </summary>
		public abstract PointF[] Vertice
		{
			get;
			set;
		}
		#endregion

		#region functions
		/// <summary>
		/// A abstract clone method because the Surface class is abstract and cannot be instanciated
		/// </summary>
		/// <returns>a conform copy of this instance of Surface</returns>
		public abstract Surface Clone();

		/// <summary>
		/// Tells if the given point is inside this surface, assuming the point and the surface are
		/// in the same coordinate system.
		/// </summary>
		/// <param name="point">the point coordinate to test</param>
		/// <returns>true if the point is inside this surface</returns>
		public abstract bool IsPointInside(PointF point);

		/// <summary>
		/// Tells if the specified axis aligned rectangle is intersecting or overlapping this surface,
		/// assuming the rectangle and the surface are in the same coordinate system.
		/// </summary>
		/// <param name="rectangle">the rectangle to test</param>
		/// <returns>true if the rectangle intersects or overlaps this surface</returns>
		public abstract bool IsRectangleIntersect(RectangleF rectangle);

		/// <summary>
		/// Translate this surface from the given vector
		/// </summary>
		/// <param name="translationVector">a vector describing the translation</param>
		public abstract void Translate(PointF translationVector);
		#endregion
	}
}
