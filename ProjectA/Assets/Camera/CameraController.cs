using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool tracking;
    public GameObject trackingObj;

    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = this.transform.position - trackingObj.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (tracking)
        {
            this.transform.position = trackingObj.transform.position + offset;
        }
    }
}
