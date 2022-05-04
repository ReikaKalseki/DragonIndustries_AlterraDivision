using System;

using System.Collections.Generic;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class MathUtil {
		
		public static double py3d(Vector3 from, Vector3 to) {
			return py3d(from.x, from.y, from.z, to.x, to.y, to.z);
		}
		
	    public static double py3d(double rawX, double rawY, double rawZ, double rawX2, double rawY2, double rawZ2) {
	    	double dx = rawX2-rawX;
	    	double dy = rawY2-rawY;
	    	double dz = rawZ2-rawZ;
	    	return py3d(dx, dy, dz);
	    }
		
	    public static double py3d(double dx, double dy, double dz) {
	    	return Math.Sqrt(dx*dx+dy*dy+dz*dz);
	    }
		
		public static Vector3 findRandomPointInsideEllipse(Vector3 center, float length, float width) {
			Rect rec = new Rect(center.x-length/2, center.z-width/2, length, width);
			Vector2 vec = getRandomVectorInside(rec);
			//SBUtil.log(rec.ToString());
			int i = 0;
			while (!isPointInsideEllipse(vec.x-center.x, 0, vec.y-center.z, length/2, 0, width/2)) {
				double ra = length/2;
				double rc = width/2;
				double x = vec.x-center.x;
				double z = vec.y-center.z;
				//SBUtil.log("Need new pos @ "+i+", vec "+vec+" failed for "+rec.xMin+">"+rec.xMax+" , "+rec.yMin+">"+rec.yMax+" = "+((x*x)/(ra*ra))+" & "+((z*z)/(rc*rc)));
				vec = getRandomVectorInside(rec);
				i++;
			}
			return new Vector3(vec.x, center.y, vec.y);
		}
		
		public static Vector2 getRandomVectorInside(Rect rec) {
			float x = UnityEngine.Random.Range(rec.xMin, rec.xMax);
			float z = UnityEngine.Random.Range(rec.yMin, rec.yMax);
			return new Vector2(x, z);
		}

		public static bool isPointInsideEllipse(double x, double y, double z, double ra, double rb, double rc) {
			return (ra > 0 ? ((x*x)/(ra*ra)) : 0) + (rb > 0 ? ((y*y)/(rb*rb)) : 0) + (rc > 0 ? ((z*z)/(rc*rc)) : 0) <= 1;
		}
		
		public static void rotateObjectAround(GameObject go, Vector3 point, double amt) {
			go.transform.RotateAround(point, Vector3.up, (float)amt);
		}
		
		public static Vector3 getRandomVectorBetween(Vector3 min, Vector3 max) {
			return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
		}
		
		public static Vector3 getRandomVectorAround(Vector3 pos, float range) {
			return getRandomVectorAround(pos, new Vector3(range, range, range));
		}
		
		public static Vector3 getRandomVectorAround(Vector3 pos, Vector3 range) {
			return getRandomVectorBetween(pos-range, pos+range);
		}
		
		public static double getDistanceToLine(Vector3 point, Vector3 a, Vector3 b) {
			return getDistanceToLine(point, a.x, a.y, a.z, b.x, b.y, b.z);
		}
		//just like when I did it for ChokePoint: https://wikimedia.org/api/rest_v1/media/math/render/svg/aad3f60fa75c4e1dcbe3c1d3a3792803b6e78bf6
		public static double getDistanceToLine(Vector3 point, double x1, double y1, double z1, double x2, double y2, double z2) {
			double denom = (x2-x1)*(x2-x1)+(z2-z1)*(z2-z1);
			double num = (x2-x1)*(z1-point.z)-(x1-point.x)*(z2-z1);
			return Math.Abs(num)/Math.Sqrt(denom);
		}

		public static double linterpolate(double x, double x1, double x2, double y1, double y2) {
			return y1+(x-x1)/(x2-x1)*(y2-y1);
		}

		public static Vector3 interpolate(Vector3 a, Vector3 b, float amt) {
			return a+(b-a)*amt;
		}
	
		public static float getRandomPlusMinus(float val, float range) {
			return UnityEngine.Random.Range(val-range, val+range);
		}
	
		public static int getRandomPlusMinus(int val, int range) {
			return UnityEngine.Random.Range(val-range, val+range);
		}
		
	}
}
