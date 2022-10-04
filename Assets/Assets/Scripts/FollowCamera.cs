using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform  Target;
    [SerializeField] private Vector3    Offset;
    private Camera main;

    Vector3 StartPos = new Vector3();
    Vector3 EndPos = new Vector3();

    private void Awake()
    {
        main = GetComponent<Camera>();
    }

    void Start()
    {
        Offset = new Vector3(0.0f, 5.0f, -15.0f);
        transform.Rotate(10.0f, 0.0f, 0.0f);

        transform.parent = Target.transform;
    }


    void Update()
    {
        // 각도가 바뀌는 것
        //transform.position = Target.position + Offset;
        transform.position = Vector3.Lerp(transform.position, Target.position + Offset, Time.deltaTime * 2.0f);

        Vector3 CameraAngles = transform.rotation.eulerAngles;
        CameraAngles.y = Input.GetAxis("Mouse X") * 10.0f;

        // 오일러 각도를 사용하는 것으로는 짐벌락 현상을 막을 수 없으므로 쿼터니언을 사용하게 된다.
        Quaternion CameraQuaternion = Quaternion.Euler(CameraAngles);

        // ** 카메라 워크 부드럽게 하기
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            CameraQuaternion,
            Time.deltaTime * 10.0f);

        if (Input.GetMouseButtonDown(1))
        {
            StartPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            EndPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            EndPos = Input.mousePosition;



            StartPos = new Vector3(0.0f, 0.0f, 0.0f);
            EndPos = new Vector3(0.0f, 0.0f, 0.0f);
        }

        Debug.Log(StartPos + " ,  " + EndPos);
    }
}
