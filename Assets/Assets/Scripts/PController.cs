using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PController : MonoBehaviour
{
    public float Speed;
    public float Rotate;

    //[SerializeField] private Animator Anim;
    [SerializeField] private GameObject MissilePrefab;
    [SerializeField] private GameObject SparkPrefab;
    [SerializeField] private GameObject SmokePrefab;
    [SerializeField] private Transform FirePointObject;
    private GameObject Head;

    public GameObject BulletObject;

    // ** 처음 한 번만 생성(호출)할 거면 이걸 쓴다
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        MissilePrefab = ((GameObject)Resources.Load("Prefabs/Missile"));
        MissilePrefab = ((GameObject)Resources.Load("Particle/Electric"));
        MissilePrefab = ((GameObject)Resources.Load("Particle/Explosion"));

        Head = transform.Find("Tower").gameObject;

        //Anim = this.transform.GetComponent<Animator>();
        
    }

    // ** 실행할 때 마다 생성(호출)하고 싶다면 이걸 쓴다
    // Start is called before the first frame update
    void Start()
    {
        Speed = 5.0f;
        Rotate = 50.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject Obj = Instantiate(BulletObject);
            GameObject Fire1Obj = Instantiate(SparkPrefab);
            GameObject Fire2Obj = Instantiate(SmokePrefab);
            Fire1Obj.transform.position = FirePointObject.transform.position;
            Fire2Obj.transform.position = GameObject.Find("FirePoint").transform.position;

            Rigidbody Rigid = Obj.transform.GetComponent<Rigidbody>();

            Obj.transform.LookAt(FirePointObject.transform.forward);
            Rigid.AddForce(FirePointObject.transform.forward * 1500.0f);
        }


        float MouseX = Input.GetAxis("Mouse X");

        Head.transform.Rotate(0.0f, MouseX * 100 * Time.deltaTime, 0.0f);

        // ** 키 입력을 받아온다
        float fHor = Input.GetAxis("Horizontal"); // 좌우키
        float fVer = Input.GetAxis("Vertical"); // 상하키

        transform.Rotate(
            0.0f,
            fHor * Time.deltaTime * Rotate,
            0.0f);

        transform.Translate(
            0.0f,
            0.0f,
            fVer * Time.deltaTime * Speed);
    }
}