using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("MoveTarget")]
    [Tooltip("type : GameObject, 조이스틱을 사용하여 움직일 대상을 정한다.")]
    [SerializeField] private GameObject Target;

    [Header("Joystick Controller")]
    [Tooltip("type : RectTransform, 실제로 움직이는 조이스틱 패드.")]
    [SerializeField] private RectTransform Stick;
    [Tooltip("type : RectTransform, 조이스틱 아웃라인.")]
    [SerializeField] private RectTransform BackBoard;

    // ** Target이 움직일 방향
    private Vector2 Direction;

    // ** Target이 움직일 값
    private Vector3 Movement;

    // ** 반지름
    private float Radius;

    // ** 이동속도
    private float Speed;

    // ** Touch 입력 여부를 체크
    private bool TouchCheck;

    public void OnDrag(PointerEventData eventData)
    {
        // ** Drag가 시작되면 터치 입력이 활성화.
        TouchCheck = true;

        // ** 움직임 계산
        OnTouch(eventData.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ** Touch 입력이 시작되면 터치 입력이 활성화.
        TouchCheck = true;
        /*
        BackBoard.transform.position = new Vector2(
            ,
            );
         */
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ** Touch 입력이 종료되면 터치 입력이 활성화.
        TouchCheck = false;

        // ** Stick을 원위치.
        Stick.localPosition = Vector2.zero;

        BackBoard.transform.position = new Vector2(
            250.0f,
            250.0f);
    }

    private void Awake()
    {
        Target = GameObject.Find("Tank");
        Stick = GameObject.Find("Image").GetComponent<RectTransform>();
        BackBoard = GameObject.Find("OutLineCircle").GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // ** Outline의 반지름을 구함
        Radius = BackBoard.rect.width * 0.5f;

        // ** 반지름의 절반 만큼 반지름을 늘린다.
        // ** 이유: Stick이 Outline을 살짝 넘어갈 수 있게 하기 위함
        //Radius += Radius * 0.5f;

        // ** Screen에 Touch가 되었는지 확인.
        TouchCheck = false;

        // ** 방향이 없는 상태로 초기화
        Direction = new Vector2(0.0f, 0.0f);

        // ** 이동 속도 설정
        Speed = 5.0f;

        // ** 이동값이 없는 상태로 초기화
        Movement = new Vector3(0.0f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        // ** Touch가 감지되면 Target을 움직인다.
        if (TouchCheck)
            Target.transform.position += Movement;
    }

    private void OnTouch(Vector2 _eventData)
    {
        // ** Stick의 중앙을 기준으로 Touch가 Screen을 이동한 거리를 구현;
        Stick.localPosition = new Vector2(_eventData.x - BackBoard.position.x, _eventData.y - BackBoard.position.y);

        // ** Stick이 Radius를 벗어나지 못하게 함
        Stick.localPosition = Vector2.ClampMagnitude(Stick.localPosition, Radius);

        // ** 1. 조이스틱이 움직이는 방향에 맞게 Target 을 이동시켜준다.

        // ** 조이스틱이 가리키고 있는 방향을 구한다
        Direction = Stick.localPosition.normalized;

        // ** 조이스틱이 움직이는 방향에 맞게 타겟을 이동시켜준다

        // ** 조이스틱이 이동 가능한 최대 거리에서 실제 이동한 비율만큼의 속도를 적용시킨다.

        float Ratio = Vector3.Distance(BackBoard.position, Stick.position) / Radius * (-1);

        //Ratio = (Ratio * 100) / Radius;

        Movement = new Vector3(
            Direction.x * (Ratio * Speed) * Time.deltaTime * (-1),
            0.0f,
            Direction.y * (Ratio * Speed) * Time.deltaTime * (-1));

        // ** 조이스틱이 바라보는 방향으로 Target을 바라보게한다.
        Target.transform.eulerAngles = new Vector3(0.0f, Mathf.Atan2(Direction.x, Direction.y) * Mathf.Rad2Deg, 0.0f);


    }
}
