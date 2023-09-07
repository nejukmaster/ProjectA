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

[_Utility.cs_](https://github.com/nejukmaster/ProjectA/blob/main/ProjectA/Assets/Utility/Scripts/Utility.cs)
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
And Then, On the Camera Controller, write a code that moves the camera along the curve.

_CameraController.cs_
```c#
//Set it to Serializable for modification in Inspector.
[System.Serializable]
public class CameraMoveCurve
{
    public string name;  //name of this curve
    public bool stareTarget;  //Whether to look at the character during this curve
    public BezierCurve3D.Curve curve;  //Curve data

    public CameraMoveCurve Clone()
    {
        CameraMoveCurve r = new CameraMoveCurve();
        r.name = name;
        r.curve = curve.Clone();
        r.stareTarget = stareTarget;
        return r;
    }
}

public CameraMoveCurve[] movingCurves;  //Array to store the curve you created

...
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

...
/*  Coroutine to move the camera along the curve.
    moveCurve  :   A curve to move the camera 
    time       :   Time to take a curve(seconds)
    isTesting  :   Whether this movement is a test. If true, return the camera to its original position after the movement.
    preProcess :   Method for Preprocessing. Before moving, It can change the properties of the Camera Controller, or apply preprocessing to the temporary curve.
    postProcess:   Method for Postprocessing. It runs after the movement is finished.  */
IEnumerator CameraMoveCo(CameraMoveCurve moveCurve, float time, bool isTesting, Action<BezierCurve3D.Curve,CameraController> preProcess, Action postProcess)
{
  updateEditor = false;  //Aborts the update of the Custom Editor on the camera controller. This will be explained later.
  float currentTime = 0f;  //Variable to store the time corutin has progressed
  BezierCurve3D.Curve curve = new BezierCurve3D.Curve();  //create temporary curve to move camera
  BezierCurve3D.BezierPoint[] points = new BezierCurve3D.BezierPoint[moveCurve.curve.points.Length + 1];  //temporary cruve's number of points is +1 of original.
  points[0] = new BezierCurve3D.BezierPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z),
                                              new Vector3(transform.position.x, transform.position.y, transform.position.z),
                                              new Vector3(transform.position.x, transform.position.y, transform.position.z));  //set start point to Camera's position.
  for (int i = 0; i < moveCurve.curve.points.Length; i++)
  {
      points[i + 1] = moveCurve.curve.points[i].Clone();  //Copy and fill BezierPoint in the remaining part.
      points[i + 1] += transform.position;  //The CameraMoveCurve is stored as a relative coordinate for the starting point, so we should turn it into a world coordinate.
  }
  curve.points = points;  //apply points to temporary curve

  preProcess(curve, this);  //Run preprocessing method

  while (currentTime < time)  //Run only if progress time is less than time
  {
      transform.position = BezierCurve3D.GetCurves(currentTime / time, curve);  //Pass the progress with a factor so that the curve progresses over "time" seconds.
      if (moveCurve.stareTarget && trackingObj != null)  //if CameraMoveCurve's stareTarget property is true, the camera look "target" During the curve
      {
          transform.LookAt(trackingObj.transform.position + trackingObj.characterTrackingPoint);
      }
      currentTime += Time.deltaTime;  //Update currentTime
      yield return null;  //Because Coroutine is consist of IEnumerator, so we should pass return as yield
  }
  if(isTesting)  //If isTesting is true, back Camera position to start point.
  {
      transform.position = curve.points[0].point;
  }
  if (moveCurve.stareTarget && trackingObj != null)  //when the coroutine is over, if stareTarget is true, set the camera rotation to look at the target 
  {
      transform.LookAt(trackingObj.transform.position + trackingObj.characterTrackingPoint);
  }

  if (postProcess != null)  //postProcess method is not null, run it.
  {
      postProcess();
  }
  updateEditor = true;
}
```
Then we can get the following results.

[![Video Label](http://img.youtube.com/vi/yjgbgNgs45w/0.jpg)](https://youtu.be/yjgbgNgs45w)

### Camera Controller Custon Editor.
Finally, we're going to create a Unity Custom Editor for easy editing of the curves in the camera walk. The Custom Editor I want is an editor which displays each BezierPoint on the screen, and drags it to modify the curve. To do this, we first create a script to inherit UnityEditor.Editor.
```c#
using UnityEditor;

...
[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor
{}
```
Then declare the variables and override the On Inspector GUI to add the required UI.
```c#

...
BezierCurve3D.Curve drawingCurve;
BezierCurve3D.BezierPoint zeroPoint;
float time = 0;  //Saves how seconds the curve test will take.

public override void OnInspectorGUI()  //Set up the GUI within the Inspector.
{
    base.OnInspectorGUI();  //Run the OnInspectorGUI of the parent object. Without this, all Inspector GUI supported by existing Unity will disappear.
    CameraController controller = target as CameraController;  //Import the targeted script into the Camera Controller.
    GUILayout.Space(20);  //It gives 20 margins on inspector.
    GUILayout.TextArea(time + "");  //Show how seconds the curve test will take.
    time = GUILayout.HorizontalSlider(time,0f,10f);  //Create a horizontal slider to adjust the time.
    GUILayout.Space(10);
    if (GUILayout.Button("Test Curves"))  //Add a button and check if it is pressed.
    {
        controller.CameraMove(controller.movingCurves.Length - 1, time, true, null, null);  //Run Camera Curve without preprocessing or postprocessing.
    }
}
```
And write Scene Screen UI at OnSceneGUI function.
```c#
private void OnSceneGUI()
{
    CameraController controller = target as CameraController;

    Handles.color = Color.magenta;  //If you set the color to magenta, the color of the UI drawn is drawn as magenta.
    if (controller.movingCurves.Length > 0)  //Do not run if there is no CameraMoveCurve registered with the Camera Controller.
    {
        EditorGUI.BeginChangeCheck();  //Create a code block up to EndCheck to see if this code has changed the GUI.
        if (controller.updateEditor)  //Update the curve only if the editorUpdate on the Camera Controller is true. If false, the curve used will not be updated, so the previous display information will be displayed.
        {
            drawingCurve = controller.movingCurves[controller.movingCurves.Length - 1].curve.Clone() + controller.transform.position;  //the last curve in the list is target of modifying.
            zeroPoint = new BezierCurve3D.BezierPoint();  //Start Point Settings
            zeroPoint.point = Handles.FreeMoveHandle(controller.transform.position, 0.2f, Vector3.zero, Handles.CylinderHandleCap);  //Create a freeMoveHandle as 0.2 size at "point" of the starting point
            zeroPoint.preTangent = Handles.FreeMoveHandle(controller.transform.position, 0.1f, Vector3.zero, Handles.CylinderHandleCap);  //Create a freeMoveHandle as 0.1 size at preTangent of the starting point
            //FreeMoveHandle dosent be created at postTangent of the starting point, Because starting point's postTangent dosent be used for drawing bezier curve.
        }

        for (int i = 0; i < drawingCurve.points.Length; i++)  //Repeat for all BezierPoints in the curve
        {
            Handles.color = Color.green;  //Set color to green to draw postTangent's FreeMoveHandle.
            drawingCurve.points[i].postTangent = Handles.FreeMoveHandle(drawingCurve.points[i].postTangent, .1f, Vector3.zero, Handles.CylinderHandleCap);
            if (i != drawingCurve.points.Length - 1)  //if If this BezierPoint is not the last of the curve
            {
                Handles.color = Color.magenta;  //Set color to magenta to draw point's FreeMoveHandle.
                drawingCurve.points[i].point = Handles.FreeMoveHandle(drawingCurve.points[i].point, .2f, Vector3.zero, Handles.CylinderHandleCap);
                Handles.color = Color.red;  //Set color to red to draw preTangent's FreeMoveHandle.
                drawingCurve.points[i].preTangent = Handles.FreeMoveHandle(drawingCurve.points[i].preTangent, .1f, Vector3.zero, Handles.CylinderHandleCap);
            }
            else  //Run at the last one
            {
                Handles.color = Color.magenta;
                drawingCurve.points[i].point = Handles.FreeMoveHandle(drawingCurve.points[i].point, .2f, Vector3.zero, Handles.CylinderHandleCap);
                drawingCurve.points[i].preTangent = drawingCurve.points[i].point;
                //last one doesn't draw postTangent's FreeMoveHandle, Because it also doesn't be used for drawing bezier curve.
            }
            //Draw a line connecting each point of one BezierPoint.
            Handles.color = Color.white;  
            Handles.DrawLine(drawingCurve.points[i].postTangent, drawingCurve.points[i].point);
            Handles.DrawLine(drawingCurve.points[i].preTangent, drawingCurve.points[i].point);
        }

        if (drawingCurve.points.Length > 0)  //If there is a BezierPoint of curve
        {
            //DrawBezier(start point, end point, start tangent, end tangent, color, texture, width) is draw cubic Bezier with points, texture, and color as width.
            Handles.DrawBezier(zeroPoint.point, drawingCurve.points[0].point, zeroPoint.preTangent, drawingCurve.points[0].postTangent, Color.gray, null, 10f);  
            for (int i = 0; i < drawingCurve.points.Length - 1; i++)
            {
                Handles.DrawBezier(drawingCurve.points[i].point, drawingCurve.points[i + 1].point, drawingCurve.points[i].preTangent, drawingCurve.points[i + 1].postTangent, Color.gray, null, 10f);
            }
        }
        BezierCurve3D.Curve resultCurve = drawingCurve.Clone();  //Variable to return modified Curve to Editor
        for (int i = 0; i < drawingCurve.points.Length; i++)
        {
            resultCurve.points[i] -= controller.transform.position;  //Turns the coordinates of the points into relative coordinates.
        }
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(controller, "Camera Curve Changed");
            if(controller.updateEditor)  //Apply changes only if updateEditor is true
                controller.movingCurves[controller.movingCurves.Length - 1].curve = resultCurve;
        }
    }
}
```
With the Unity Custom Editor built in this way, we can modify and apply the camera walk more intuitively.

[![Video Label](http://img.youtube.com/vi/gKEHt0coLOE/0.jpg)](https://youtu.be/gKEHt0coLOE)
