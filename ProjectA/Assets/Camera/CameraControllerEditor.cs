using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor
{
    BazierCurve3D.Curve drawingCurve;
    BazierCurve3D.BazierPoint zeroPoint;
    float time = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CameraController controller = target as CameraController;
        GUILayout.Space(20);
        GUILayout.TextArea(time + "");
        time = GUILayout.HorizontalSlider(time,0f,10f);
        GUILayout.Space(10);
        if (GUILayout.Button("Test Curves"))
        {
            controller.CameraMove(controller.movingCurves.Length - 1, time, true, null, null);
        }
    }

    private void OnSceneGUI()
    {
        CameraController controller = (CameraController)target;

        Handles.color = Color.magenta;
        if (controller.movingCurves.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            if (controller.updateEditor)
            {
                drawingCurve = controller.movingCurves[controller.movingCurves.Length - 1].curve.Clone() + controller.transform.position;
                zeroPoint = new BazierCurve3D.BazierPoint();
                zeroPoint.point = Handles.FreeMoveHandle(controller.transform.position, 0.2f, Vector3.zero, Handles.CylinderHandleCap);
                zeroPoint.preTangent = Handles.FreeMoveHandle(controller.transform.position, 0.1f, Vector3.zero, Handles.CylinderHandleCap);
            }

            for (int i = 0; i < drawingCurve.points.Length; i++)
            {
                Handles.color = Color.green;
                drawingCurve.points[i].postTangent = Handles.FreeMoveHandle(drawingCurve.points[i].postTangent, .1f, Vector3.zero, Handles.CylinderHandleCap);
                if (i != drawingCurve.points.Length - 1)
                {
                    Handles.color = Color.magenta;
                    drawingCurve.points[i].point = Handles.FreeMoveHandle(drawingCurve.points[i].point, .2f, Vector3.zero, Handles.CylinderHandleCap);
                    Handles.color = Color.red;
                    drawingCurve.points[i].preTangent = Handles.FreeMoveHandle(drawingCurve.points[i].preTangent, .1f, Vector3.zero, Handles.CylinderHandleCap);
                }
                else
                {
                    Handles.color = Color.magenta;
                    drawingCurve.points[i].point = Handles.FreeMoveHandle(drawingCurve.points[i].point, .2f, Vector3.zero, Handles.CylinderHandleCap);
                    drawingCurve.points[i].preTangent = drawingCurve.points[i].point;
                }
                Handles.color = Color.white;
                Handles.DrawLine(drawingCurve.points[i].postTangent, drawingCurve.points[i].point);
                Handles.DrawLine(drawingCurve.points[i].preTangent, drawingCurve.points[i].point);
            }

            if (drawingCurve.points.Length > 0)
            {
                Handles.DrawBezier(zeroPoint.point, drawingCurve.points[0].point, zeroPoint.preTangent, drawingCurve.points[0].postTangent, Color.gray, null, 10f);
                for (int i = 0; i < drawingCurve.points.Length - 1; i++)
                {
                    Handles.DrawBezier(drawingCurve.points[i].point, drawingCurve.points[i + 1].point, drawingCurve.points[i].preTangent, drawingCurve.points[i + 1].postTangent, Color.gray, null, 10f);
                }
            }
            BazierCurve3D.Curve resultCurve = drawingCurve.Clone();
            for (int i = 0; i < drawingCurve.points.Length; i++)
            {
                resultCurve.points[i] -= controller.transform.position;
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(controller, "Camera Curve Changed");
                if(controller.updateEditor)
                    controller.movingCurves[controller.movingCurves.Length - 1].curve = resultCurve;
            }
        }
    }

    Vector3 UpdateCameraPos()
    {
        CameraController controller = target as CameraController;
        if (controller.updateEditor) return controller.transform.position;
        else return Vector3.zero;
    }
}
