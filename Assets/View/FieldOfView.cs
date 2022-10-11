using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public struct CastInfo
    {
        // ** �¾Ҵ��� Ȯ��
        public bool Hit;

        // ** �¾Ҵٸ� ���� ��ġ, �ȸ¾Ҵٸ� Radius�� �Ÿ�
        public Vector3 Point;

        // ** ���� �Ÿ�
        public float Distance;

        // ** ����
        public float Angle;
    }
        
    [Header("Circle")]
    [Range(0, 30)]
    public float Radius = 0;
    
    [Range(0, 360)]
    public float ViewAngle = 0.0f;

    [Range(0.1f, 5.0f)]
    public float Angle = 0.5f;
    public float OffsetAngle = 0;

    [HideInInspector] public List<CastInfo> LineList = new List<CastInfo>();

    [HideInInspector] public List<Transform> TargetList = new List<Transform>();

    
    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private LayerMask ObstacleMask;

    public MeshFilter _MeshFilter;
    private Mesh _Mesh;



    //bool Check = true;

    private void Awake()
    {
        _MeshFilter = GameObject.Find("View").transform.GetComponent<MeshFilter>();

        _Mesh = new Mesh();
        _MeshFilter.mesh = _Mesh;
    }


    private void Start()
    {
        Radius = 25.0f;
        ViewAngle = 90.0f;
        Angle = 5.0f;

        StartCoroutine(CheckTargetat());
    }

    private void Update()
    {
        GetVertex();

        
        if (Input.GetKey(KeyCode.LeftArrow))
            OffsetAngle -= 5;

        if (Input.GetKey(KeyCode.RightArrow))
            OffsetAngle += 5;
        

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(
                transform.up * (OffsetAngle * 0.5f)),
            2.0f * Time.deltaTime);
    }

    public Vector3 GetEulerAngle(float _Angle)
    {
        return new Vector3( 
            Mathf.Sin(_Angle * Mathf.Deg2Rad), 
            0.0f, 
            Mathf.Cos(_Angle * Mathf.Deg2Rad)) * Radius + transform.position;
    }
    IEnumerator CheckTargetat()
    {
        while (true)
        {
            TargetList.Clear();

            Collider[] ColList = Physics.OverlapSphere(transform.position, Radius, TargetMask);

            foreach(Collider element in ColList)
            {
                Vector3 Direction = (element.transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, Direction) < (ViewAngle * 0.5f))
                {
                    float fDistance = Vector3.Distance(transform.position, element.transform.position);

                    if (!Physics.Raycast(transform.position, Direction, fDistance, ObstacleMask))
                        TargetList.Add(element.transform);
                }
            }
            yield return new WaitForSeconds(0.1f);
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
            // ** �¾Ҵ��� �ɸ¾Ҵ��� Ȯ��
            Info.Hit = true;

            // ** ����
            Info.Angle = _Angle;

            // ** �Ÿ�
            Info.Distance = hit.distance;

            // ** ���� ��ġ
            Info.Point = hit.point;
        }
        else
        {
            // ** �¾Ҵ��� �ɸ¾Ҵ��� Ȯ��
            Info.Hit = false;

            // ** ����
            Info.Angle = _Angle;

            // ** �ȸ¾����Ƿ� �ش� ������ �ִ� �Ÿ��� Radius�� ��ġ
            Info.Point = transform.position + Direction * Radius;

            // ** �ȸ¾����Ƿ� �ִ� ���� �Ÿ��� Radius
            Info.Distance = Radius;
        }
        return Info;
    }

    void GetVertex()
    {
        LineList.Clear();

        

        int Count = Mathf.RoundToInt(ViewAngle / Angle) + 1;
        float fAngle = - transform.eulerAngles.y - ((ViewAngle - OffsetAngle) * 0.5f);



        for (int i = 0; i < Count; ++i)
        {
            CastInfo Info = GetCastInfo(fAngle + (Angle * i));
            LineList.Add(Info);
        }

        int VertexCount = LineList.Count + 1;

        Vector3[] Vertices = new Vector3[VertexCount];
        int[] Triangles = new int[(VertexCount - 2) * 3];


        Vertices[0] = Vector3.zero;

        for (int i = 1; i < Vertices.Length; ++i)
            Vertices[i] = LineList[i - 1].Point - transform.position;

        //Debug.Log(LineList.Count + ", " + Vertices.Length);

        for (int i = 0; i < Vertices.Length - 2; ++i)
        {
            Triangles[i * 3] = 0;
            Triangles[i * 3 + 1] = i + 1;
            Triangles[i * 3 + 2] = i + 2;
        }

        _Mesh.Clear();

        _Mesh.vertices = Vertices;
        _Mesh.triangles = Triangles;

        _Mesh.RecalculateNormals();
    }
}