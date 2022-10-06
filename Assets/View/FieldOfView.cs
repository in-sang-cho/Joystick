using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Circle")]
    [Range(0, 30)]
    public float Radius = 0;

    [Range(0, 360)]
    public float ViewAngle = 0.0f;

    [Range(0.1f, 5.0f)]
    public float Angle;
    public float OffsetAngle = 0;

    [HideInInspector] public List<Vector3> LineList = new List<Vector3>();

    [HideInInspector] public List<Transform> TargetList = new List<Transform>();

    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private LayerMask ObstacleMask;

    private void Start()
    {
        Radius = 25.0f;
        ViewAngle = 90.0f;

        Angle = 0.5f;


        StartCoroutine(CheckTarget());
    }

    private void Update()
    {
        LineList.Clear();

        int Count = Mathf.RoundToInt(ViewAngle / Angle) + 1;

        float fAngle = -(ViewAngle * 0.5f) + 1;

        for (int i = 0; i < Count; ++i)
        {
            Vector3 v = new Vector3(
                Mathf.Sin((fAngle + (Angle * i)) * Mathf.Deg2Rad),
                0.0f,
                Mathf.Cos((fAngle + (Angle * i)) * Mathf.Deg2Rad));

            RaycastHit hit;

            if(Physics.Raycast(transform.position, v, out hit, Radius, ObstacleMask))
                LineList.Add(hit.point);
            else
                LineList.Add(v * Radius);

        }

        if (Input.GetKey(KeyCode.LeftArrow))
            OffsetAngle -= 5;

        if (Input.GetKey(KeyCode.RightArrow))
            OffsetAngle += 5;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(transform.up * (ViewAngle + OffsetAngle) * 0.5f),
            (2.0f * Time.deltaTime));

    }

    public Vector3 GetEulerAngle(float _Angle)
    {
        return new Vector3(
            Mathf.Sin(_Angle * Mathf.Deg2Rad),
            0.0f,
            Mathf.Cos(_Angle * Mathf.Deg2Rad)) * Radius + transform.position;
    }
    
    IEnumerator CheckTarget()
    {
        TargetList.Clear();


        while(true)
        {

            Collider[] ColList = Physics.OverlapSphere(transform.position, Radius, TargetMask);

            //Debug.Log(ColList.Length);

            foreach (Collider element in ColList)
            {
                Vector3 Direction = (element.transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, Direction) < (ViewAngle * 0.5f))
                {

                    float vDistance = Vector3.Distance(transform.position, element.transform.position);

                    if (!Physics.Raycast(transform.position, Direction, vDistance, ObstacleMask))
                        TargetList.Add(element.transform);
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}
