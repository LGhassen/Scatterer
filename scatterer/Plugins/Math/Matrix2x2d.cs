
public class Matrix2x2d  
{
	//Members varibles
	
	public double[,] m = new double[2,2];
	
	//Constructors
	
	public Matrix2x2d() {}
	
	public Matrix2x2d(double m00, double m01, double m10, double m11)
	{
		m[0,0] = m00; m[0,1] = m01;
    	m[1,0] = m10; m[1,1] = m11;
	}
	
	public Matrix2x2d(double[,] m)
	{
		System.Array.Copy(m, this.m, 4);
	}
	
	public Matrix2x2d(Matrix2x2d m)
	{
		System.Array.Copy(m.m, this.m, 4);
	}
	
	//Operator Overloads
	
	public static Matrix2x2d operator +(Matrix2x2d m1, Matrix2x2d m2) 
   	{
		Matrix2x2d kSum = new Matrix2x2d();
		for (int iRow = 0; iRow < 2; iRow++) {
			for (int iCol = 0; iCol < 2; iCol++) {
		    	kSum.m[iRow,iCol] = m1.m[iRow,iCol] + m2.m[iRow,iCol];
			}
		}
		return kSum;
   	}
	
	public static Matrix2x2d operator -(Matrix2x2d m1, Matrix2x2d m2) 
   	{
		Matrix2x2d kSum = new Matrix2x2d();
		for (int iRow = 0; iRow < 2; iRow++) {
			for (int iCol = 0; iCol < 2; iCol++) {
		    	kSum.m[iRow,iCol] = m1.m[iRow,iCol] - m2.m[iRow,iCol];
			}
		}
		return kSum;
   	}
	
	public static Matrix2x2d operator *(Matrix2x2d m1, Matrix2x2d m2) 
   	{
		Matrix2x2d kProd = new Matrix2x2d();
		for (int iRow = 0; iRow < 2; iRow++) {
			for (int iCol = 0; iCol < 2; iCol++) {
		    	kProd.m[iRow,iCol] = m1.m[iRow,0] * m2.m[0,iCol] + m1.m[iRow,1] * m2.m[1,iCol];
			}
		}
		return kProd;
   	}
	
	public static Vector2d operator *(Matrix2x2d m, Vector2d v)
	{
	    Vector2d kProd = new Vector2d();
	  
	    kProd.x = m.m[0,0] * v.x + m.m[0,1] * v.y;
		kProd.y = m.m[1,0] * v.x + m.m[1,1] * v.y;
	    
	    return kProd;
	}
	
	public static Matrix2x2d operator *(Matrix2x2d m, double s)
	{
		Matrix2x2d kProd = new Matrix2x2d();
		for (int iRow = 0; iRow < 2; iRow++) {
			for (int iCol = 0; iCol < 2; iCol++) {
		    	kProd.m[iRow,iCol] = m.m[iRow,iCol] * s;
			}
		}
		return kProd;
	}
	
	//Functions
	
	public override string ToString()
   	{
		return m[0,0] + "," + m[0,1] + "\n" + m[1,0] + "," + m[1,1];
   	}
	
	public Matrix2x2d Transpose()
	{
	    Matrix2x2d kTranspose = new Matrix2x2d();
	    for (int iRow = 0; iRow < 2; iRow++) {
	        for (int iCol = 0; iCol < 2; iCol++) {
	            kTranspose.m[iRow,iCol] = m[iCol,iRow];
	        }
	    }
	    return kTranspose;
	}
	
	private double Determinant()
	{
	    return m[0,0] * m[1,1] - m[1,0] * m[0,1];
	}
	
	//public bool Inverse(ref Matrix2x2d mInv, double tolerance = 1e-06)
	public bool Inverse(ref Matrix2x2d mInv, double tolerance)
	{
	    double det = Determinant();
	
	    if (System.Math.Abs(det) <= tolerance) {
	        return false;
	    }
	
	    double invDet = 1.0 / det;
	
	    mInv.m[0,0] = m[1,1] * invDet;
	    mInv.m[0,1] = -m[0,1] * invDet;
	    mInv.m[1,0] = -m[1,0] * invDet;
	    mInv.m[1,1] = m[0,0] * invDet;
	    return true;
	}
	
	//public Matrix2x2d Inverse(double tolerance = 1e-06)
	public Matrix2x2d Inverse(double tolerance)
	{
	    Matrix2x2d kInverse = new Matrix2x2d();
	    Inverse(ref kInverse, tolerance);
	    return kInverse;
	}
	
	static public Matrix2x2d Identity()
	{
		return new Matrix2x2d(1,0,0,1);
	}

}

























