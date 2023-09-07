using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public class CameraMoveCurve
    {
        public string name;
        public bool stareTarget;
        public BezierCurve3D.Curve curve;

        public CameraMoveCurve Clone()
        {
            CameraMoveCurve r = new CameraMoveCurve();
            r.name = name;
            r.curve = curve.Clone();
            r.stareTarget = stareTarget;
            return r;
        }
    }

    public bool tracking = false;
    public PlayerController trackingObj;
    [Range(0f,100f)]
    public float mouseSensitivity;
    [Range(0f, 100f)]
    public float zoomSensitivity;
    public CameraMoveCurve[] movingCurves;
    public bool updateEditor = true;
    public Coroutine currentCo;

    [SerializeField] float zoomLimits;

    private float radious;
    private Vector3 offset;

    // Update is called once per frame
    void Update()
    {
        if (tracking)
        {
            this.transform.position = (trackingObj.transform.position + trackingObj.characterTrackingPoint) + offset;
            this.transform.LookAt(trackingObj.transform.position + trackingObj.characterTrackingPoint);
            float xAxis = Input.GetAxis("Mouse X");
            float yAxis = Input.GetAxis("Mouse Y");

            Vector3 spherical = Utility.TranslateOrthogonalToSpherical(offset);
            if (radious - Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity > zoomLimits)
                radious -= Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
            spherical.x = radious;
            spherical.y += yAxis * Time.deltaTime * mouseSensitivity;
            spherical.z -= xAxis * Time.deltaTime * mouseSensitivity;

            Ray ray = new Ray((trackingObj.transform.position + trackingObj.characterTrackingPoint), transform.position - (trackingObj.transform.position + trackingObj.characterTrackingPoint));
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            if (hit.collider != null)
            {
                Vector3 hitpos = hit.point;
                float hitdis = Vector3.Distance(hitpos, (trackingObj.transform.position + trackingObj.characterTrackingPoint));
                if (hitdis < radious) spherical.x = hitdis;
            }

            offset = Utility.TranslateSphericalToOrthogonal(spherical);
        }
    }

    public void SetTrackingObj(PlayerController player)
    {
        trackingObj = player;

        offset = this.transform.position - (trackingObj.transform.position + trackingObj.characterTrackingPoint);
        radious = Vector3.Distance(this.transform.position, (trackingObj.transform.position + trackingObj.characterTrackingPoint));

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        tracking = true;
    }

    [ExecuteInEditMode]
    public void CameraMove(int index, float time, bool isTesting, Action<BezierCurve3D.Curve, CameraController> preProcess, Action postProcess)
    {
        if (time > 0 && index > 0 && index < movingCurves.Length)
        {
            Action<BezierCurve3D.Curve, CameraController> t_pre = preProcess;
            if (t_pre == null)
                t_pre = (curve, controller) => { };
            currentCo = StartCoroutine(CameraMoveCo(movingCurves[index], time, isTesting, t_pre, null));
        }
    }

    public void CameraMove(string name, float time, bool isTesting, Action<BezierCurve3D.Curve, CameraController> preProcess, Action postProcess)
    {
        if (time > 0)
        {
            Action<BezierCurve3D.Curve, CameraController> t_pre = preProcess;
            if (t_pre == null)
                t_pre = (curve, controller) => { };
            for (int i = 0; i < movingCurves.Length; i++)
            {
                if (movingCurves[i].name == name)
                {
                    currentCo = StartCoroutine(CameraMoveCo(movingCurves[i], time, isTesting, t_pre, null));
                    break;
                }
            }
        }
    }

    IEnumerator CameraMoveCo(CameraMoveCurve moveCurve, float time, bool isTesting, Action<BezierCurve3D.Curve,CameraController> preProcess, Action postProcess)
    {
        updateEditor = false;
        float currentTime = 0f;
        BezierCurve3D.Curve curve = new BezierCurve3D.Curve();
        BezierCurve3D.BezierPoint[] points = new BezierCurve3D.BezierPoint[moveCurve.curve.points.Length + 1];
        points[0] = new BezierCurve3D.BezierPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z),
                                                    new Vector3(transform.position.x, transform.position.y, transform.position.z),
                                                    new Vector3(transform.position.x, transform.position.y, transform.position.z));
        for (int i = 0; i < moveCurve.curve.points.Length; i++)
        {
            points[i + 1] = moveCurve.curve.points[i].Clone();
            points[i + 1] += transform.position;
        }
        curve.points = points;

        preProcess(curve, this);

        while (currentTime < time)
        {
            transform.position = BezierCurve3D.GetCurves(currentTime / time, curve);
            if (moveCurve.stareTarget && trackingObj != null)
            {
                transform.LookAt(trackingObj.transform.position + trackingObj.characterTrackingPoint);
            }
            currentTime += Time.deltaTime;
            yield return null;
        }
        //if (!isTesting)
        //    transform.position = moveCurve.curve.points[moveCurve.curve.points.Length - 1].point;
        if(isTesting)
        {
            transform.position = curve.points[0].point;
        }
        if (moveCurve.stareTarget && trackingObj != null)
        {
            transform.LookAt(trackingObj.transform.position + trackingObj.characterTrackingPoint);
        }

        if (postProcess != null)
        {
            postProcess();
        }
        updateEditor = true;
    }
}
