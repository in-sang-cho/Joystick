using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FieldOfView))]
public class ViewEditor : Editor
{
    private void OnSceneGUI()
    {
        // ** FieldOfView Target ����
        FieldOfView Target = (FieldOfView)target;

        // ** GUI�� ������� ����
        Handles.color = Color.green;

        for (int i = 0; i < Target.LineList.Count; ++i)
        {
            Handles.DrawLine(Target.transform.position, Target.LineList[i].Point);
        }

        Handles.color = Color.red;

        Handles.DrawWireArc(Target.transform.position, Vector3.up, Vector3.forward, 360.0f, Target.Radius);

        // ** ���� Endline
        Vector3 LeftLine = Target.GetEulerAngle(-(Target.ViewAngle - Target.OffsetAngle) * 0.5f);

        Handles.DrawLine(Target.transform.position, LeftLine);

        // ** ������ Endline
        Vector3 RightLine = Target.GetEulerAngle((Target.ViewAngle - Target.OffsetAngle) * 0.5f);

        Handles.DrawLine(Target.transform.position, RightLine);

        Handles.color = Color.red;

        foreach (Transform element in Target.TargetList)
        {
            Handles.DrawLine(Target.transform.position, element.position);
        }
    }
}