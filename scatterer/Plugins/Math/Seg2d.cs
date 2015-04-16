using UnityEngine;
using System.Collections;

public class Seg2d 
{
	
    //One of the segment extremity.
	public Vector2d a;

    //The vector joining #a to the other segment extremity.
	public Vector2d ab;
	
	/**
     * Creates a new segment with the given extremities.
     *
     * @param a a segment extremity.
     * @param b the other segment extremity.
     */
	public Seg2d(Vector2d a, Vector2d b)
	{
		this.a = new Vector2d(a);
		this.ab = b-a;
	}
	
	/**
     * Returns the square distance between the given point and the line
     * defined by this segment.
     *
     * @param p a point.
     */
	public double LineDistSq(Vector2d p) 
	{
		Vector2d ap = p - a;
		double dotprod = ab.Dot(ap);
		double projLenSq = dotprod * dotprod / ab.SqrMagnitude();
		return ap.SqrMagnitude() - projLenSq;
	}
	
	/**
     * Returns the square distance between the given point and this segment.
     *
     * @param p a point.
     */
	public double SegmentDistSq(Vector2d p) 
	{
		Vector2d ap = p - a;
		double dotprod = ab.Dot(ap);
		double projlenSq;
		
		if (dotprod <= 0.0) {
			projlenSq = 0.0;
		} 
		else {
			ap = ab - ap;
			dotprod = ab.Dot(ap);
			
			if (dotprod <= 0.0) {
				projlenSq = 0.0;
			} 
			else {
				projlenSq = dotprod * dotprod / ab.SqrMagnitude();
			}
		}
		
		return ap.SqrMagnitude() - projlenSq;
	}
	
	/**
     * Returns true if this segment intersects the given segment.
     *
     * @param s a segment.
     */
	public bool Intersects(Seg2d s) 
	{
		Vector2d aa = s.a - a;
		double det = Vector2d.Cross(ab, s.ab);
		double t0 = Vector2d.Cross(aa, s.ab) / det;
		
		if (t0 > 0 && t0 < 1) {
			double t1 = Vector2d.Cross(aa, ab) / det;
			return t1 > 0 && t1 < 1;
		}
		
		return false;
	}
	
	public bool Intersects(Seg2d s, double t0)
	{
		Vector2d aa = s.a - a;
		double det = Vector2d.Cross(ab, s.ab);
		t0 = Vector2d.Cross(aa, s.ab) / det;
		
		if (t0 > 0 && t0 < 1) {
			double t1 = Vector2d.Cross(aa, ab) / det;
			return t1 > 0 && t1 < 1;
		}
		
		return false;
	}
	
	/**
     * Returns true if this segment intersects the given segment. If there
     * is an intersection it is returned in the vector.
     *
     * @param s a segment.
     * @param i where to store the intersection point, if any.
     */
	public bool Intersects(Seg2d s, ref Vector2d i)
	{
		Vector2d aa = s.a - a;
		double det = Vector2d.Cross(ab, s.ab);
		double t0 = Vector2d.Cross(aa, s.ab) / det;
		
		if (t0 > 0 && t0 < 1) {
			i = a + ab * t0;
			double t1 = Vector2d.Cross(aa, ab) / det;
			return t1 > 0 && t1 < 1;
		}
		
		return false;
	}
	
	/**
     * Returns true if this segment is inside or intersects the given
     * bounding box.
     *
     * @param r a bounding box.
     */
	public bool Intersects(Box2d r)
	{
		Vector2d b = a + ab;
		if (r.Contains(a) || r.Contains(b)) {
			return true;
		}
		
		Box2d t = new Box2d(a, b);
		if (t.xmin > r.xmax || t.xmax < r.xmin || t.ymin > r.ymax || t.ymax < r.ymin) {
			return false;
		}
		
		double p0 = Vector2d.Cross(ab, new Vector2d(r.xmin, r.ymin) - a);
		double p1 = Vector2d.Cross(ab, new Vector2d(r.xmax, r.ymin) - a);
		if (p1 * p0 <= 0) {
			return true;
		}
		double p2 = Vector2d.Cross(ab, new Vector2d(r.xmin, r.ymax) - a);
		if (p2 * p0 <= 0) {
			return true;
		}
		double p3 = Vector2d.Cross(ab, new Vector2d(r.xmax, r.ymax) - a);
		if (p3 * p0 <= 0) {
			return true;
		}
		
		return false;
	}
	
	/**
     * Returns true if this segment, with the given width, contains the given
     * point. More precisely this method returns true if the stroked path
     * defined from this segment, with a cap "butt" style, contains the given
     * point.
     *
     * @param p a point.
     * @param w width of this segment.
     */
	public bool Contains(Vector2d p, double w)
	{
		Vector2d ap = p - a;
		double dotprod = ab.Dot(ap);
		double projlenSq;
		
		if (dotprod <= 0.0) {
			return false;
		} 
		else {
			ap = ab - ap;
			dotprod = ab.Dot(ap);
			if (dotprod <= 0.0) {
				return false;
			} 
			else {
				projlenSq = dotprod * dotprod / ab.SqrMagnitude();
				return ap.SqrMagnitude() - projlenSq < w*w;
			}
		}
	}



}














