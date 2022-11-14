using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Transform characterBody;
    [SerializeField]
    private Transform cameraArm;

    [SerializeField] 
    private float speed;
    [SerializeField] 
    private float rotate;
    [SerializeField] 
    private GameObject[] weapons;
    [SerializeField] 
    private bool[] hasWeapons;
    [SerializeField] 
    private GameObject[] berserker;
    [SerializeField] 
    private int hasBerserker;

    [SerializeField] 
    private int health;
    [SerializeField] 
    private int stamina;
    [SerializeField] 
    private int berserk;

    [SerializeField] 
    private int maxHealth;
    [SerializeField] 
    private int maxStamina;
    [SerializeField] 
    private int maxBerserk;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown;
    bool aDown;

    bool isMove;
    bool isJump;
    bool isRoll;
    bool isSwap;
    bool isAtkReady;
    bool isBorder;

    Vector3 moveVec;
    Vector3 rollVec;

    Rigidbody rigid;
    Animator anim;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float atkDelay;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        LookAround();
        GetInput();
        Move();
        Jump();
        Attack();
        Roll();
        Interation();
        Swap();
    }

    void LookAround()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 camAngle = cameraArm.rotation.eulerAngles;

        float x = camAngle.x - mouseDelta.y;

        if (x < 180.0f)
            x = Mathf.Clamp(x, -1f, 70f);
        else
            x = Mathf.Clamp(x, 335f, 361f);

        cameraArm.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);
        cameraArm.position = characterBody.position;
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButton("Jump");
        iDown = Input.GetButton("Interation");
        sDown = Input.GetButton("Switch1");
        aDown = Input.GetButton("Fire1");
    }

    void Move()
    {
        Vector2 moveInput = new Vector2(hAxis, vAxis);
        isMove = moveInput.magnitude != 0;
        anim.SetBool("isRun", isMove);
        
        //anim.SetBool("isRun", moveVec != Vector3.zero);
        if (isMove)
        {
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

            characterBody.forward = lookForward;
            transform.position += moveDir * Time.deltaTime * speed;
        }

        if (isRoll)
            moveVec = rollVec;
        if (isSwap || ((!isAtkReady && equipWeapon != null ) && !isJump) || isBorder)
            moveVec = Vector3.zero;


        if (!isBorder)
            transform.position += moveVec * speed * (wDown ? 0.1f : 0.2f) * Time.deltaTime;
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isRoll && !isSwap)
        {
            rigid.AddForce(Vector3.up * 16, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;

        atkDelay += Time.deltaTime;
        isAtkReady = equipWeapon.rate < atkDelay;

        if (aDown && isAtkReady && !isRoll && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger("doAttack");
            atkDelay = 0;
        }
    }

    void Roll()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isRoll && !isSwap)
        {
            speed *= 2;
            rollVec = moveVec;
            anim.SetTrigger("doRoll");
            isRoll = true;

            Invoke("RollOut", 0.4f);
        }
    }

    void RollOut()
    {
        speed *= 0.5f;
        isRoll = false;

    }

    void Interation()
    {
        if (iDown && nearObject != null && !isJump && !isRoll)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    void Swap()
    {
        if (sDown && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;

        int weaponIndex = -1;
        if (sDown) weaponIndex = 0;

        if (sDown && !isJump && !isRoll)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }    
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Potion")
        {
            Item potion = other.GetComponent<Item>();
            switch(potion.type)
            {
                case Item.Type.Health:
                    health += potion.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Stamina:
                    stamina += potion.value;
                    if (stamina > maxStamina)
                        stamina = maxStamina;
                    break;
                case Item.Type.Berserk:
                    berserker[hasBerserker].SetActive(true);
                    berserk += potion.value;
                    if (berserk > maxBerserk)
                        berserk = maxBerserk;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = other.gameObject;

        //Debug.Log(nearObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        
    }
}
