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


        // ** LeftLine, RightLine, EndLine이 월드좌표를 기준으로 그리는 것을 로컬로 변경
        // ** 회전이 안됨
        // ** ViewField 내부에서만 target을 find 할 수 있도록


        float lAngle = (-Target.ViewAngle / 2);

        Vector3 LeftLine = new Vector3(
            Mathf.Sin(lAngle * Mathf.Deg2Rad),
            0.0f,
            Mathf.Cos(lAngle * Mathf.Deg2Rad) ) * Target.Radius;

        Handles.DrawLine(Target.transform.position, LeftLine);

        float rAngle = (Target.ViewAngle / 2);

        Vector3 RightLine = new Vector3(
            Mathf.Sin(rAngle * Mathf.Deg2Rad),
            0.0f,
            Mathf.Cos(rAngle * Mathf.Deg2Rad) ) * Target.Radius;

        Handles.DrawLine(Target.transform.position, RightLine);

        foreach (Transform element in Target.TargetList)
        {
            Handles.DrawLine(Target.transform.position, element.position);
        }
    }
}