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
        Handles.color = Color.red;

        Handles.DrawWireArc(Target.transform.position, Vector3.up, Vector3.forward, 360.0f, Target.Radius);


        // ** LeftLine, RightLine, EndLine�� ������ǥ�� �������� �׸��� ���� ���÷� ����
        // ** ȸ���� �ȵ�
        // ** ViewField ���ο����� target�� find �� �� �ֵ���


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