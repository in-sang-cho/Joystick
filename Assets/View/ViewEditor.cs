using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FieldOfView))]
public class ViewEditor : Editor
{
    private void OnSceneGUI()
    {
        // ** FieldOfView Target 설정
        FieldOfView Target = (FieldOfView)target;

        // ** GUI를 흰색으로 설정
        Handles.color = Color.red;

        Handles.DrawWireArc(Target.transform.position, Vector3.up, Vector3.forward, 360.0f, Target.Radius);

        // ** 정면 방향
        Handles.DrawLine(Target.transform.position, Target.transform.forward * Target.Radius);

        // ** 왼쪽 Endline
        Vector3 LeftLine = Target.GetEulerAngle(-(Target.ViewAngle - Target.Angle) + 0.5f);

        Handles.DrawLine(Target.transform.position, LeftLine);

        // ** 오른쪽 Endline
        Vector3 RightLine = Target.GetEulerAngle((Target.ViewAngle - Target.Angle) + 0.5f);

        Handles.DrawLine(Target.transform.position, RightLine);

        Handles.color = Color.green;

        foreach (Transform element in Target.TargetList)
        {
            Handles.DrawLine(Target.transform.position, element.position);
        }
    }
}