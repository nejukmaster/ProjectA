Camera Controller
=================
The Camera Controller is a script that controls the movement of the camera during the game. The main functions of this script are as follows.
> Rotating around the character
>
> Track characters
>
> Camera Walk

### Rotating around the character
In Open World games, the camera has to move freely than the other games. Therefore, I made a camera that rotates around the character in a circle according to the movement of the mouse.

The basic idea is to add vertical and horizontal movements of the mouse to the spherical coordinates of the camera, respectively, and change them back into orthogonal coordinate systems. 
![Alt text](/ExplainImgs/SphericalCoordinateForCamera.png)
To implement this, we first make a function that transforms the orthogonal coordinate system and the spherical coordinate system.

_Utility.cs_
```c#
public static Vector3 TranslateOrthogonalToSpherical(Vector3 p_vec)
{
  Vector3 r = Vector3.zero;
  r.x = p_vec.magnitude;
  r.y = Mathf.Acos(p_vec.y/ p_vec.magnitude);
  r.z = Mathf.Atan2(p_vec.z , p_vec.x);
  return r;
}

public static Vector3 TranslateSphericalToOrthogonal(Vector3 p_vec)
{
  return new Vector3(p_vec.x * Mathf.Sin(p_vec.y) * Mathf.Cos(p_vec.z), p_vec.x * Mathf.Cos(p_vec.y), p_vec.x * Mathf.Sin(p_vec.y)*Mathf.Sin(p_vec.z));
}
```
Then write an update block.

_CameraController.cs_
```c#
void Update()
{
  if (tracking)//Run only when tracking is enabled
  {
    this.transform.position = (trackingObj.transform.position + trackingObj.characterTrackingPoint) + offset; //Set location to track. offset is the relative position of the camera relative to the character.
    this.transform.LookAt(trackingObj.transform.position + trackingObj.characterTrackingPoint);
    float xAxis = Input.GetAxis("Mouse X");  //Horizontal axis movement of the mouse
    float yAxis = Input.GetAxis("Mouse Y");  //the vertical axis movement of the mouse

    Vector3 spherical = Utility.TranslateOrthogonalToSpherical(offset);  //Transform orthogonal coordinate system into spherical coordinate system
    if (radious - Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity > zoomLimits)
      radious -= Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;  //Set radius with mouse wheel

    spherical.x = radious;                                      //r value set
    spherical.y += yAxis * Time.deltaTime * mouseSensitivity;   //Φ+ΔΦ value set
    spherical.z -= xAxis * Time.deltaTime * mouseSensitivity;   //θ+Δθ value set

    /*  Use raycast to check for shielding between the camera and the character  */
    Ray ray = new Ray((trackingObj.transform.position + trackingObj.characterTrackingPoint), transform.position - (trackingObj.transform.position + trackingObj.characterTrackingPoint));
    RaycastHit hit;
    Physics.Raycast(ray, out hit);

    if (hit.collider != null)
    {
      Vector3 hitpos = hit.point;
      float hitdis = Vector3.Distance(hitpos, (trackingObj.transform.position + trackingObj.characterTrackingPoint));
      if (hitdis < radious) spherical.x = hitdis;  
    }
  
    offset = Utility.TranslateSphericalToOrthogonal(spherical);  //Transform spherical coordinate system to orthogonal coordinate system
  }
}
```
