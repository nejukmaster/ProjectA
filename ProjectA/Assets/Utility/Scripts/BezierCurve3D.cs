using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BezierCurve3D
{
    [System.Serializable]
    public class BezierPoint
    {
        public static BezierPoint operator -(BezierPoint a) => new BezierPoint(-a.point, -a.postTangent, -a.preTangent);
        public static BezierPoint operator +(BezierPoint a, Vector3 b) => new BezierPoint(a .point + b, a.postTangent + b, a.preTangent + b);
        public static BezierPoint operator +(BezierPoint a, BezierPoint b) => new BezierPoint(a.point + b.point, a.postTangent + b.postTangent, a.preTangent + b.preTangent);
        public static BezierPoint operator -(BezierPoint a, Vector3 b) => new BezierPoint(a.point - b, a.postTangent - b, a.preTangent - b);
        public static BezierPoint operator -(BezierPoint a, BezierPoint b) => new BezierPoint(a.point - b.point, a.postTangent - b.postTangent, a.preTangent - b.preTangent);
        public static BezierPoint operator *(BezierPoint a, Vector3 b) => new BezierPoint(new Vector3(a.point.x * b.x, a.point.y * b.y, a.point.z * b.z), new Vector3(a.postTangent.x * b.x, a.postTangent.y * b.y, a.postTangent.z * b.z), new Vector3(a.preTangent.x * b.x, a.preTangent.y * b.y, a.preTangent.z * b.z));
        public static BezierPoint operator *(BezierPoint a, BezierPoint b) => new BezierPoint(new Vector3(a.point.x * b.point.x, a.point.y * b.point.y, a.point.z * b.point.z), new Vector3(a.postTangent.x * b.postTangent.x, a.postTangent.y * b.postTangent.y, a.postTangent.z * b.postTangent.z), new Vector3(a.preTangent.x * b.preTangent.x, a.preTangent.y * b.preTangent.y, a.preTangent.z * b.preTangent.z));

        public Vector3 point;
        public Vector3 postTangent;
        public Vector3 preTangent;

        public BezierPoint(Vector3 point, Vector3 postTangent, Vector3 preTangent)
        {
            this.point = point;
            this.postTangent = postTangent;
            this.preTangent = preTangent;
        }

        public BezierPoint() { }

        public BezierPoint Clone()
        {
            return new BezierPoint(point, postTangent, preTangent);
        }

        public BezierPoint Inverse()
        {
            return new BezierPoint(new Vector3(point.x == 0 ? 0 : 1 / point.x, point.y == 0 ? 0 : 1 / point.y, point.z == 0 ? 0 : 1 / point.z),
                                    new Vector3(postTangent.x == 0 ? 0 : 1 / postTangent.x, postTangent.y == 0 ? 0 : 1 / postTangent.y, postTangent.z == 0 ? 0 : 1 / postTangent.z),
                                    new Vector3(preTangent.x == 0 ? 0 : 1 / preTangent.x, preTangent.y == 0 ? 0 : 1 / preTangent.y, preTangent.z == 0 ? 0 : 1 / preTangent.z));
        }
    }
    [System.Serializable]
    public class Curve
    {
        public static Curve operator +(Curve a, Vector3 b) => PlusOperate(a,b);
        public static Curve operator -(Curve a, Vector3 b) => PlusOperate(a, -b);

        public static Curve PlusOperate(Curve a, Vector3 b)
        {
            Curve r = a.Clone();
            for(int i = 0; i < r.points.Length; i ++)
            {
                r.points[i] += b;
            }
            return r;
        }

        public BezierPoint[] points;

        public Curve Clone()
        {
            Curve curve = new Curve();
            curve.points = new BezierPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                curve.points[i] = points[i].Clone();
            }
            return curve;
        }
    }

    public static Vector3 GetCurves(float t, Curve curve)
    {
        if (curve.points.Length <= 0)
            return Vector3.zero;
        else
        {
            float _t = t * (float)(curve.points.Length - 1);
            BezierPoint postPoint = curve.points[(int)_t];
            BezierPoint prePoint = curve.points[(int)_t+1];

            Vector3 p1 = Vector3.Lerp(postPoint.point, postPoint.preTangent, _t - Mathf.Floor(_t));
            Vector3 p2 = Vector3.Lerp(postPoint.preTangent, prePoint.postTangent, _t - Mathf.Floor(_t));
            Vector3 p3 = Vector3.Lerp(prePoint.postTangent, prePoint.point, _t - Mathf.Floor(_t));
            Vector3 p4 = Vector3.Lerp(p1, p2, _t - Mathf.Floor(_t));
            Vector3 p5 = Vector3.Lerp(p2, p3, _t - Mathf.Floor(_t));

            return Vector3.Lerp(p4, p5, _t - Mathf.Floor(_t));
        }
    }
}
