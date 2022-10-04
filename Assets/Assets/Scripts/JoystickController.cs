using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("MoveTarget")]
    [Tooltip("type : GameObject, ���̽�ƽ�� ����Ͽ� ������ ����� ���Ѵ�.")]
    [SerializeField] private GameObject Target;

    [Header("Joystick Controller")]
    [Tooltip("type : RectTransform, ������ �����̴� ���̽�ƽ �е�.")]
    [SerializeField] private RectTransform Stick;
    [Tooltip("type : RectTransform, ���̽�ƽ �ƿ�����.")]
    [SerializeField] private RectTransform BackBoard;

    // ** Target�� ������ ����
    private Vector2 Direction;

    // ** Target�� ������ ��
    private Vector3 Movement;

    // ** ������
    private float Radius;

    // ** �̵��ӵ�
    private float Speed;

    // ** Touch �Է� ���θ� üũ
    private bool TouchCheck;

    public void OnDrag(PointerEventData eventData)
    {
        // ** Drag�� ���۵Ǹ� ��ġ �Է��� Ȱ��ȭ.
        TouchCheck = true;

        // ** ������ ���
        OnTouch(eventData.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ** Touch �Է��� ���۵Ǹ� ��ġ �Է��� Ȱ��ȭ.
        TouchCheck = true;
        /*
        BackBoard.transform.position = new Vector2(
            ,
            );
         */
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ** Touch �Է��� ����Ǹ� ��ġ �Է��� Ȱ��ȭ.
        TouchCheck = false;

        // ** Stick�� ����ġ.
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
        // ** Outline�� �������� ����
        Radius = BackBoard.rect.width * 0.5f;

        // ** �������� ���� ��ŭ �������� �ø���.
        // ** ����: Stick�� Outline�� ��¦ �Ѿ �� �ְ� �ϱ� ����
        //Radius += Radius * 0.5f;

        // ** Screen�� Touch�� �Ǿ����� Ȯ��.
        TouchCheck = false;

        // ** ������ ���� ���·� �ʱ�ȭ
        Direction = new Vector2(0.0f, 0.0f);

        // ** �̵� �ӵ� ����
        Speed = 5.0f;

        // ** �̵����� ���� ���·� �ʱ�ȭ
        Movement = new Vector3(0.0f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        // ** Touch�� �����Ǹ� Target�� �����δ�.
        if (TouchCheck)
            Target.transform.position += Movement;
    }

    private void OnTouch(Vector2 _eventData)
    {
        // ** Stick�� �߾��� �������� Touch�� Screen�� �̵��� �Ÿ��� ����;
        Stick.localPosition = new Vector2(_eventData.x - BackBoard.position.x, _eventData.y - BackBoard.position.y);

        // ** Stick�� Radius�� ����� ���ϰ� ��
        Stick.localPosition = Vector2.ClampMagnitude(Stick.localPosition, Radius);

        // ** 1. ���̽�ƽ�� �����̴� ���⿡ �°� Target �� �̵������ش�.

        // ** ���̽�ƽ�� ����Ű�� �ִ� ������ ���Ѵ�
        Direction = Stick.localPosition.normalized;

        // ** ���̽�ƽ�� �����̴� ���⿡ �°� Ÿ���� �̵������ش�

        // ** ���̽�ƽ�� �̵� ������ �ִ� �Ÿ����� ���� �̵��� ������ŭ�� �ӵ��� �����Ų��.

        float Ratio = Vector3.Distance(BackBoard.position, Stick.position) / Radius * (-1);

        //Ratio = (Ratio * 100) / Radius;

        Movement = new Vector3(
            Direction.x * (Ratio * Speed) * Time.deltaTime * (-1),
            0.0f,
            Direction.y * (Ratio * Speed) * Time.deltaTime * (-1));

        // ** ���̽�ƽ�� �ٶ󺸴� �������� Target�� �ٶ󺸰��Ѵ�.
        Target.transform.eulerAngles = new Vector3(0.0f, Mathf.Atan2(Direction.x, Direction.y) * Mathf.Rad2Deg, 0.0f);


    }
}
