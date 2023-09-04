Camera Controller
=================
The Camera Controller is a script that controls the movement of the camera during the game. The main functions of this script are as follows.
> Rotating around the character & Tracking character
>
> Camera Walk
>
> Custom Editor

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
And make additional functions that set the object to be tracked.
```c#
public void SetTrackingObj(PlayerController player)
{
  trackingObj = player;

  offset = this.transform.position - (trackingObj.transform.position + trackingObj.characterTrackingPoint);  //Initialize offset
  radious = Vector3.Distance(this.transform.position, (trackingObj.transform.position + trackingObj.characterTrackingPoint));

  //Hide and position the mouse cursor so that the mouse does not go off-screen.
  Cursor.visible = false;
  Cursor.lockState = CursorLockMode.Locked;
  tracking = true;  //start tracking
}
```
And this is the camera movement video.

[![Video Label](http://img.youtube.com/vi/g6W0aXs5C28/0.jpg)](https://youtu.be/g6W0aXs5C28)
### Camera Walk
Camera Walk consists of 3D Bezier. Bezier Curve is a curve defined by two or more points and is widely used in vector graphics. Using this Bezier curve, the camera's transit point will be stored, and each transit point will be interpolated into a Cubic Bezier curve to form a camera walk.

At first, We make the Curve class and its composition BezierPoint class. The structure of the Curve class is as follows.
![Alt text](/ExplainImgs/BazierCurve3DSchematicDiagram.png)

_Bezier Point class_
```c#
[System.Serializable]
public class BezierPoint
{
  //Four basic operations override
  public static BezierPoint operator -(BezierPoint a) => new BezierPoint(-a.point, -a.postTangent, -a.preTangent);
  public static BezierPoint operator +(BezierPoint a, Vector3 b) => new BezierPoint(a .point + b, a.postTangent + b, a.preTangent + b);
  public static BezierPoint operator +(BezierPoint a, BezierPoint b) => new BezierPoint(a.point + b.point, a.postTangent + b.postTangent, a.preTangent + b.preTangent);
  public static BezierPoint operator -(BezierPoint a, Vector3 b) => new BezierPoint(a.point - b, a.postTangent - b, a.preTangent - b);
  public static BezierPoint operator -(BezierPoint a, BezierPoint b) => new BezierPoint(a.point - b.point, a.postTangent - b.postTangent, a.preTangent - b.preTangent);
  public static BezierPoint operator *(BezierPoint a, Vector3 b) => new BezierPoint(new Vector3(a.point.x * b.x, a.point.y * b.y, a.point.z * b.z), new Vector3(a.postTangent.x * b.x, a.postTangent.y * b.y, a.postTangent.z * b.z), new Vector3(a.preTangent.x * b.x, a.preTangent.y * b.y, a.preTangent.z * b.z));
  public static BezierPoint operator *(BezierPoint a, BezierPoint b) => new BezierPoint(new Vector3(a.point.x * b.point.x, a.point.y * b.point.y, a.point.z * b.point.z), new Vector3(a.postTangent.x * b.postTangent.x, a.postTangent.y * b.postTangent.y, a.postTangent.z * b.postTangent.z), new Vector3(a.preTangent.x * b.preTangent.x, a.preTangent.y * b.preTangent.y, a.preTangent.z * b.preTangent.z));

  //points to consist bezier curve
  public Vector3 point;
  public Vector3 postTangent;
  public Vector3 preTangent;

  //cunstructor
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

  //Caculate and return reciprocal
  public BezierPoint Inverse()
  {
      return new BezierPoint(new Vector3(point.x == 0 ? 0 : 1 / point.x, point.y == 0 ? 0 : 1 / point.y, point.z == 0 ? 0 : 1 / point.z),
                              new Vector3(postTangent.x == 0 ? 0 : 1 / postTangent.x, postTangent.y == 0 ? 0 : 1 / postTangent.y, postTangent.z == 0 ? 0 : 1 / postTangent.z),
                              new Vector3(preTangent.x == 0 ? 0 : 1 / preTangent.x, preTangent.y == 0 ? 0 : 1 / preTangent.y, preTangent.z == 0 ? 0 : 1 / preTangent.z));
  }
}
```
_Curve Class_
```c#
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
```
Now, given the position t in Curve, we create a GetCurve function that returns the point P above the Bezier Curve.
_GetCurve Function_
```c#
//input t is a value of 0~1
public static Vector3 GetCurves(float t, Curve curve)
{
  //If Curve is blank, return zero vector.
  if (curve.points.Length <= 0)
    return Vector3.zero;
  else
  {
    float _t = t * (float)(curve.points.Length - 1);  //mapping t 0~1 to 0~number of curve's points
    BezierPoint postPoint = curve.points[(int)_t];  //using _t, get bezier point right in front of current position.
    BezierPoint prePoint = curve.points[(int)_t+1];  //ustin _t, get bezier point next of current position.

    //calculate cubic bezier curve
    Vector3 p1 = Vector3.Lerp(postPoint.point, postPoint.preTangent, _t - Mathf.Floor(_t));
    Vector3 p2 = Vector3.Lerp(postPoint.preTangent, prePoint.postTangent, _t - Mathf.Floor(_t));
    Vector3 p3 = Vector3.Lerp(prePoint.postTangent, prePoint.point, _t - Mathf.Floor(_t));
    Vector3 p4 = Vector3.Lerp(p1, p2, _t - Mathf.Floor(_t));
    Vector3 p5 = Vector3.Lerp(p2, p3, _t - Mathf.Floor(_t));

    return Vector3.Lerp(p4, p5, _t - Mathf.Floor(_t));
  }
}
```
