using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [HideInInspector] public float Radius;
    [HideInInspector] public float ViewAngle;

    [HideInInspector] public List<Transform> TargetList = new List<Transform>();

    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private LayerMask ObstacleMask;

    private void Start()
    {
        Radius = 30;

        ViewAngle = 90.0f;

        StartCoroutine(CheckTarget());
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
               
                float vDistance = Vector3.Distance(transform.position, element.transform.position);

                if (!Physics.Raycast(transform.position, element.transform.position, Radius, ObstacleMask))
                {
                    TargetList.Add(element.transform);
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}
