using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;          // ** 앞뒤 이동 속도
    public float rotateSpeed = 180.0f;      // ** 좌우 회전 속도

    private PlayerInput playerInput;        // ** 플레이어 입력을 알려주는 컴포넌트
    [SerializeField] private Rigidbody playerRigidBody;      // ** 플레이어의 리지드바디
    private Animator playerAnimator;        // ** 플레이어의 애니메이터

    private void Start()
    {
        // ** 사용할 플레이어의 컴포넌트 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidBody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Rotate();

        Move();

        playerAnimator.SetFloat("Move", playerInput.move);

    }

    private void Move()
    {
        // ** 상대적으로 이동할 거리 계산
        Vector3 moveDistance = playerInput.move * transform.forward * moveSpeed * Time.deltaTime;

        // ** 리지드바디를 통해 게임 오브젝트 위치 변경
        playerRigidBody.MovePosition(playerRigidBody.position + moveDistance);
    }

    private void Rotate()
    {
        // ** 리지드바디를 통해 게임 오브젝트 회전 변경
        playerRigidBody.rotation = playerRigidBody.rotation * Quaternion.Euler(0.0f, 1.0f, 0.0f);
    }
}
