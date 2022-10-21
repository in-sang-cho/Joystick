using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public string moveAxisName = "Vertical";        // ** 앞뒤 움직임을 위한 입력축 이름
    public string rotateAxisName = "Horizontal";    // ** 좌우 회전을 위한 입력축 이름

    public float move { get; private set; }         // ** 감지된 움직임 입력 값
    public float rotate { get; private set; }       // ** 감지된 회전 입력 값

    private void Update()
    {
        move = Input.GetAxis(moveAxisName);
        rotate = Input.GetAxis(rotateAxisName);
    }
}
