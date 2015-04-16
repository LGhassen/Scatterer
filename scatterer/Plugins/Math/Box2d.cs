

public class Box2d
{
	//Members varibles
	
	public double xmin, xmax, ymin, ymax;
	
	//Constructors
	
	public Box2d()
    {
		xmin = double.PositiveInfinity;
		xmax = double.NegativeInfinity;
		ymin = double.PositiveInfinity;
		ymax = double.NegativeInfinity;
    }

    public Box2d(double xmin, double xmax, double ymin, double ymax)
    {
		this.xmin = xmin;
		this.xmax = xmax;
		this.ymin = ymin;
		this.ymax = ymax;
    }

    public Box2d(Vector2d p, Vector2d q)
    {
		xmin = System.Math.Min(p.x,q.x);
		xmax = System.Math.Max(p.x,q.x);
		ymin = System.Math.Min(p.y,q.y);
		ymax = System.Math.Max(p.y,q.y);
    }
	
	//Functions
	
    public Vector2d Center()
    {
        return new Vector2d((xmin + xmax) / 2.0, (ymin + ymax) / 2.0);
    }

    public double Width()
    {
        return xmax - xmin;
    }

    public double Height()
    {
        return ymax - ymin;
    }

    public double Area()
    {
        return (xmax - xmin) * (ymax - ymin);
    }
	
	//Returns the bounding box containing this box and the given border.
	public Box2d Enlarge(double w)
    {
        return new Box2d(xmin - w, xmax + w, ymin - w, ymax + w);
    }

    
    //Returns the bounding box containing this box and the given point.
    public Box2d Enlarge(Vector2d p)
    {
        return new Box2d(System.Math.Min(xmin, p.x), System.Math.Max(xmax, p.x), System.Math.Min(ymin, p.y), System.Math.Max(ymax, p.y));
    }

    
    //Returns the bounding box containing this box and the given box.
    public Box2d Enlarge(Box2d r)
    {
        return new Box2d(System.Math.Min(xmin, r.xmin), System.Math.Max(xmax, r.xmax), System.Math.Min(ymin, r.ymin), System.Math.Max(ymax, r.ymax));
    }
	
    //Returns true if this bounding box contains the given point.
    public bool Contains(Vector2d p)
    {
        return (p.x >= xmin && p.x <= xmax && p.y >= ymin && p.y <= ymax);
    }

    
    //Returns true if this bounding box contains the given bounding box.
    public bool Contains(Box2d bb)
    {
        return (bb.xmin >= xmin && bb.xmax <= xmax && bb.ymin >= ymin && bb.ymax <= ymax);
    }

    //Alias for clipRectangle.
    public bool Intersects(Box2d a)
    {
        return (a.xmax >= xmin && a.xmin <= xmax && a.ymax >= ymin && a.ymin <= ymax);
    }

    
    //Returns the nearest point to a contained in the box.
    public Vector2d NearestInnerPoint(Vector2d a)
    {
        Vector2d nearest = new Vector2d(a);
        if (a.x < xmin) {
            nearest.x = xmin;
        } else if (a.x > xmax) {
            nearest.x = xmax;
        }

        if (a.y < ymin) {
            nearest.y = ymin;
        } else if (a.y > ymax) {
            nearest.y = ymax;
        }
        return nearest;
    }

}

















