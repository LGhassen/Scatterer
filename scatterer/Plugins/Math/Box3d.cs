
public class Box3d
{
	//Members varibles
	
	public double xmin, xmax, ymin, ymax, zmin, zmax;
	
	//Constructors
	
	public Box3d()
    {
		xmin = double.PositiveInfinity;
		xmax = double.NegativeInfinity;
		ymin = double.PositiveInfinity;
		ymax = double.NegativeInfinity;
		zmin = double.PositiveInfinity;
		zmax = double.NegativeInfinity;
    }

    public Box3d(double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
    {
		this.xmin = xmin;
		this.xmax = xmax;
		this.ymin = ymin;
		this.ymax = ymax;
		this.zmin = zmin;
		this.zmax = zmax;
    }

    public Box3d(Vector3d2 p, Vector3d2 q)
    {
		xmin = System.Math.Min(p.x,q.x);
		xmax = System.Math.Max(p.x,q.x);
		ymin = System.Math.Min(p.y,q.y);
		ymax = System.Math.Max(p.y,q.y);
		zmin = System.Math.Min(p.z,q.z);
		zmax = System.Math.Max(p.z,q.z);
    }
	
	//Functions
	
    public Vector3d2 Center()
    {
        return new Vector3d2((xmin + xmax) / 2.0, (ymin + ymax) / 2.0, (zmin + zmax) / 2.0);
    }

    
    //Returns the bounding box containing this box and the given point.
    public Box3d Enlarge(Vector3d2 p)
    {
        return new Box3d(	System.Math.Min(xmin, p.x), System.Math.Max(xmax, p.x), 
							System.Math.Min(ymin, p.y), System.Math.Max(ymax, p.y), 
							System.Math.Min(zmin, p.z), System.Math.Max(zmax, p.z));
    }

    
    //Returns the bounding box containing this box and the given box.
    public Box3d Enlarge(Box3d r)
    {
        return new Box3d(	System.Math.Min(xmin, r.xmin), System.Math.Max(xmax, r.xmax), 
							System.Math.Min(ymin, r.ymin), System.Math.Max(ymax, r.ymax), 
							System.Math.Min(zmin, r.zmin), System.Math.Max(zmax, r.zmax));
    }

    
    //Returns true if this bounding box contains the given point.
    public bool Contains(Vector3d2 p)
    {
        return (p.x >= xmin && p.x <= xmax && p.y >= ymin && p.y <= ymax && p.z >=zmin && p.z <= zmax);
    }


}




















