using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Utility
{
    public static Vector3 TranslateCertesianToSpherical(Vector3 p_vec)
    {
        Vector3 r = Vector3.zero;
        r.x = p_vec.magnitude;
        r.y = Mathf.Acos(p_vec.y/ p_vec.magnitude);
        r.z = Mathf.Atan2(p_vec.z , p_vec.x);
        return r;
    }

    public static Vector3 TranslateSphericalToCertesian(Vector3 p_vec)
    {
        return new Vector3(p_vec.x * Mathf.Sin(p_vec.y) * Mathf.Cos(p_vec.z), p_vec.x * Mathf.Cos(p_vec.y), p_vec.x * Mathf.Sin(p_vec.y)*Mathf.Sin(p_vec.z));
    }
}
