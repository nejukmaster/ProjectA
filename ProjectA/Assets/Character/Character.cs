using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] Light mainLight;
    [SerializeField] Camera mainCamera;

    // Update is called once per frame
    [ExecuteAlways]
    void Update()
    {
        if(material != null)
        {
            if(mainCamera != null)
                material.SetVector("_ViewDir", mainCamera.transform.forward);
            if(mainLight != null)
                material.SetVector("_LitDir", mainLight.transform.forward);
        }
    }
}
