using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool tracking = false;
    public PlayerController trackingObj;
    [Range(0f,100f)]
    public float mouseSensitivity;
    [Range(0f, 100f)]
    public float zoomSensitivity;

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

            Vector3 spherical = Utility.TranslateCertesianToSpherical(offset);
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

            offset = Utility.TranslateSphericalToCertesian(spherical);
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
}
