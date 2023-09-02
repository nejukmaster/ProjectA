using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BazierCurve3D
{
    [System.Serializable]
    public class BazierPoint
    {
        public static BazierPoint operator -(BazierPoint a) => new BazierPoint(-a.point, -a.postTangent, -a.preTangent);
        public static BazierPoint operator +(BazierPoint a, Vector3 b) => new BazierPoint(a .point + b, a.postTangent + b, a.preTangent + b);
        public static BazierPoint operator +(BazierPoint a, BazierPoint b) => new BazierPoint(a.point + b.point, a.postTangent + b.postTangent, a.preTangent + b.preTangent);
        public static BazierPoint operator -(BazierPoint a, Vector3 b) => new BazierPoint(a.point - b, a.postTangent - b, a.preTangent - b);
        public static BazierPoint operator -(BazierPoint a, BazierPoint b) => new BazierPoint(a.point - b.point, a.postTangent - b.postTangent, a.preTangent - b.preTangent);
        public static BazierPoint operator *(BazierPoint a, Vector3 b) => new BazierPoint(new Vector3(a.point.x * b.x, a.point.y * b.y, a.point.z * b.z), new Vector3(a.postTangent.x * b.x, a.postTangent.y * b.y, a.postTangent.z * b.z), new Vector3(a.preTangent.x * b.x, a.preTangent.y * b.y, a.preTangent.z * b.z));
        public static BazierPoint operator *(BazierPoint a, BazierPoint b) => new BazierPoint(new Vector3(a.point.x * b.point.x, a.point.y * b.point.y, a.point.z * b.point.z), new Vector3(a.postTangent.x * b.postTangent.x, a.postTangent.y * b.postTangent.y, a.postTangent.z * b.postTangent.z), new Vector3(a.preTangent.x * b.preTangent.x, a.preTangent.y * b.preTangent.y, a.preTangent.z * b.preTangent.z));

        public Vector3 point;
        public Vector3 postTangent;
        public Vector3 preTangent;

        public BazierPoint(Vector3 point, Vector3 postTangent, Vector3 preTangent)
        {
            this.point = point;
            this.postTangent = postTangent;
            this.preTangent = preTangent;
        }

        public BazierPoint() { }

        public BazierPoint Clone()
        {
            return new BazierPoint(point, postTangent, preTangent);
        }

        public BazierPoint Inverse()
        {
            return new BazierPoint(new Vector3(point.x == 0 ? 0 : 1 / point.x, point.y == 0 ? 0 : 1 / point.y, point.z == 0 ? 0 : 1 / point.z),
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

        public BazierPoint[] points;

        public Curve Clone()
        {
            Curve curve = new Curve();
            curve.points = new BazierPoint[points.Length];
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
            BazierPoint postPoint = curve.points[(int)_t];
            BazierPoint prePoint = curve.points[(int)(_t + 1f)];

            Vector3 p1 = Vector3.Lerp(postPoint.point, postPoint.preTangent, _t - Mathf.Floor(_t));
            Vector3 p2 = Vector3.Lerp(postPoint.preTangent, prePoint.postTangent, _t - Mathf.Floor(_t));
            Vector3 p3 = Vector3.Lerp(prePoint.postTangent, prePoint.point, _t - Mathf.Floor(_t));
            Vector3 p4 = Vector3.Lerp(p1, p2, _t - Mathf.Floor(_t));
            Vector3 p5 = Vector3.Lerp(p2, p3, _t - Mathf.Floor(_t));

            return Vector3.Lerp(p4, p5, _t - Mathf.Floor(_t));
        }
    }
}
