using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;          // ** �յ� �̵� �ӵ�
    public float rotateSpeed = 180.0f;      // ** �¿� ȸ�� �ӵ�

    private PlayerInput playerInput;        // ** �÷��̾� �Է��� �˷��ִ� ������Ʈ
    [SerializeField] private Rigidbody playerRigidBody;      // ** �÷��̾��� ������ٵ�
    private Animator playerAnimator;        // ** �÷��̾��� �ִϸ�����

    private void Start()
    {
        // ** ����� �÷��̾��� ������Ʈ ��������
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
        // ** ��������� �̵��� �Ÿ� ���
        Vector3 moveDistance = playerInput.move * transform.forward * moveSpeed * Time.deltaTime;

        // ** ������ٵ� ���� ���� ������Ʈ ��ġ ����
        playerRigidBody.MovePosition(playerRigidBody.position + moveDistance);
    }

    private void Rotate()
    {
        // ** ������ٵ� ���� ���� ������Ʈ ȸ�� ����
        playerRigidBody.rotation = playerRigidBody.rotation * Quaternion.Euler(0.0f, 1.0f, 0.0f);
    }
}
