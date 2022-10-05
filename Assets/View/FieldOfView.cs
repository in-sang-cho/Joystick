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

    public float Angle = 0;

    //[HideInInspector]
    public List<Transform> TargetList = new List<Transform>();

    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private LayerMask ObstacleMask;

    private void Start()
    {
        Radius = 25.0f;

        ViewAngle = 90.0f;

        StartCoroutine(CheckTarget());
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            Angle -= 5;

        if (Input.GetKey(KeyCode.RightArrow))
            Angle += 5;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(transform.up * (ViewAngle + Angle) * 0.5f),
            (2.0f * Time.deltaTime));

    }

    public Vector3 GetEulerAngle(float _Angle)
    {
        return new Vector3(
            Mathf.Sin(_Angle * Mathf.Deg2Rad),
            0.0f,
            Mathf.Cos(_Angle * Mathf.Deg2Rad));
    }
    
    IEnumerator CheckTarget()
    {
        while(true)
        {
            TargetList.Clear();

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
