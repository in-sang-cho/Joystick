using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public struct CastInfo
    {
        // ** 맞았는지 확인
        public bool Hit;

        // ** 맞았다면 맞은 위치, 맞지 않았다면 Radius의 거리
        public Vector3 Point;

        // ** 도달 거리
        public float Distance;

        // ** 각도
        public float Angle;
    }


    [Header("Circle")]
    [Range(0, 30)]
    public float Radius = 0;

    [Range(0, 360)]
    public float ViewAngle = 0.0f;

    [Range(0.1f, 5.0f)]
    public float Angle;
    public float OffsetAngle = 0;

    [HideInInspector] public List<CastInfo> LineList = new List<CastInfo>();

    [HideInInspector] public List<Transform> TargetList = new List<Transform>();

    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private LayerMask ObstacleMask;

    //== 플레이어 컨트롤러


    public MeshFilter _MeshFilter;
    public Mesh _Mesh;



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

        /*
            */

        while (true)
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

    public Vector3 GetAngle(float _Angle)
    {
        return new Vector3(Mathf.Sin(_Angle * Mathf.Deg2Rad), 0.0f, Mathf.Cos(_Angle * Mathf.Deg2Rad));
    }

    public CastInfo GetCastInfo(float _Angle)
    {
        Vector3 Direction = GetAngle(_Angle);

        CastInfo Info;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, Direction, out hit, Radius, ObstacleMask))
        {
            // ** 맞았다면
            Info.Hit = true;

            // ** 각도
            Info.Angle = _Angle;

            // ** 거리
            Info.Distance = hit.distance;

            // ** 맞은 위치
            Info.Point = hit.point;
        }
        else
        {
            // ** 안맞았다면
            Info.Hit = false;

            // ** 각도
            Info.Angle = _Angle;

            // ** 최대 도달 거리인 Radius
            Info.Distance = Radius;

            // ** 발사 방향의 최대 거리인 Radius의 위치
            Info.Point = transform.position + Direction * Radius;
        }

        return new CastInfo();
    }




    void GetVertex()
    {
        int Count = Mathf.RoundToInt(ViewAngle / Angle);
        float fAngle = -(ViewAngle - OffsetAngle) * 0.5f;

        for (int i = 0; i > Count; ++i)
        {
            CastInfo Info = GetCastInfo(fAngle + (Angle * i));
            LineList.Add(Info);
        }

        int VertexCount = LineList.Count + 1;

        Vector3[] Vertices = new Vector3[VertexCount];
        int[] Triangles = new int[(VertexCount - 2) * 3];

        Vertices[0] = Vector3.zero;

        for (int i = 1; i < Vertices.Length; ++i)
            Vertices[i] = LineList[i - 1].Point;

        /*
        for (int i = 0; i < vertices.Length; ++i)
        {
             vertices[i * 3] = 0;
             vertices[i * 3 + 1] = i + 1;
             vertices[i * 3 + 2] = i + 2;
        }
         */
        _Mesh.Clear();

        _Mesh.vertices = Vertices;
        _Mesh.triangles = Triangles;

    }
}