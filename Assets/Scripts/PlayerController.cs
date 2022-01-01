using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{

    public Camera normalCam;
    public float runModifier;
    
    public float jumpForce;
    public Transform whereIsGround;
    public LayerMask ground;
    
    [SerializeField] private float speed;
    private Rigidbody rb;
    float FOV;//field of view
    float runFOVModifier = 1.5f;
    private Vector3 origin_cam;

    public Transform weaponParent;
    private Vector3 targetShakePosition;
    private Vector3 weaponParentOrigin;
    private Vector3 weaponParentCurrentPos;

    private float idle_shake;
    private float move_shake;

    RaycastHit slopeHit;
    float playerHeight = 2f;
    Vector3 slopeMoveDirection;

    public int max_health;
    private int current_health;

    private Transform healthbar_UI;
    private Text ammo_UI;

    private Manager manager;

    private Weapon weapon;

    public float lengthOfSlide;
    public float slideModifier;
    private bool sliding;
    private float slide_time;
    private Vector3 slide_dir;
    public float slideAmount;

    public float crouchModifier;
    public float crouchAmount;
    public GameObject standingCollider;
    public GameObject crouchingCollider;
    private bool crouched;

    private float aimAngle;

    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        weapon = GetComponent<Weapon>();
        current_health = max_health;

        rb = GetComponent<Rigidbody>();
        FOV = normalCam.fieldOfView;
        origin_cam = normalCam.transform.localPosition;

        weaponParentOrigin = weaponParent.localPosition;
        weaponParentCurrentPos = weaponParentOrigin;

        if (!photonView.IsMine)
        {
            gameObject.layer = 9;
            standingCollider.layer = 9;
            crouchingCollider.layer = 9;
        }

        if (photonView.IsMine)
        {
            healthbar_UI = GameObject.Find("HUD/Health/HealBar").transform;
            ammo_UI = GameObject.Find("HUD/Ammo/Ammotxt").GetComponent<Text>();
            RefreshHealBar();
        }
        
    }

    private void Update()
    {
        if (!photonView.IsMine) 
        {
            RefreshMultiplayerState();
            return;
        }

        float h_move = Input.GetAxisRaw("Horizontal");
        float v_move = Input.GetAxisRaw("Vertical");
        bool run = Input.GetKey(KeyCode.LeftShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool crouch = Input.GetKeyDown(KeyCode.LeftControl);

        bool isGround = Physics.Raycast(whereIsGround.position, Vector3.down, 0.2f, ground);
        bool isJumping = jump && isGround;
        bool isRunning = run && v_move > 0 && isGround;
        bool isCrouching = crouch && !isRunning && !isJumping && isGround;

        if (isCrouching)
        {
            photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
        }

        if (isJumping)
        {
            if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
            rb.AddForce(Vector3.up * jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(20);

        
        if (sliding) 
        {
            ShakeGun(move_shake, 0.075f, 0.03f);
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetShakePosition, Time.deltaTime * 10f);
        }
        else if (h_move == 0 && v_move == 0)
        {
            ShakeGun(idle_shake, 0.025f, 0.025f);
            idle_shake += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetShakePosition, Time.deltaTime * 2f);
        }
        else if (!isRunning && !crouched)
        {
            ShakeGun(move_shake, 0.05f, 0.05f);
            move_shake += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetShakePosition, Time.deltaTime * 6f);
        }
        else if (crouched)
        {
            ShakeGun(move_shake, 0.02f, 0.02f);
            move_shake += Time.deltaTime * 1.75f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetShakePosition, Time.deltaTime * 6f);
        }
        else
        {
            ShakeGun(move_shake, 0.15f, 0.15f);
            move_shake += Time.deltaTime * 7f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetShakePosition, Time.deltaTime * 10f);
        }

        //UI refresh
        RefreshHealBar();
        weapon.RefreshAmmo(ammo_UI);
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float h_move = Input.GetAxisRaw("Horizontal");
        float v_move = Input.GetAxisRaw("Vertical");
        bool run = Input.GetKey(KeyCode.LeftShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool slide = Input.GetKey(KeyCode.C);

        bool isGround = Physics.Raycast(whereIsGround.position, Vector3.down, 0.2f, ground);
        bool isJumping = jump && isGround;
        bool isRunning = run && v_move > 0 && isGround;
        bool isSlope = isGround && OnSlope();
        bool isSliding = slide && isRunning && !sliding;

        //Movement
        Vector3 dir = Vector3.zero;
        float adjustedSpeed = speed;

        if (!sliding)
        {
            dir = new Vector3(h_move, 0f, v_move);
            dir.Normalize(); //Binh thuong hoa
            dir = transform.TransformDirection(dir);

            if (isRunning)
            {
                if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                adjustedSpeed = adjustedSpeed * runModifier;
            }
            else if (crouched)
            {
                adjustedSpeed = adjustedSpeed * crouchModifier;
            }
        }
        else
        {
            dir = slide_dir;
            adjustedSpeed = adjustedSpeed * slideModifier;
            slide_time -= Time.deltaTime;
            if (slide_time <= 0)
            {
                sliding = false;
                weaponParentCurrentPos -= Vector3.down * (slideAmount - crouchAmount);
            }
        }

        Vector3 TargetVelocity = dir * adjustedSpeed * Time.deltaTime;
        TargetVelocity.y = rb.velocity.y;
        rb.velocity = TargetVelocity;

        //Sliding
        if (isSliding)
        {
            sliding = true;
            slide_dir = dir;
            slide_time = lengthOfSlide;

            weaponParentCurrentPos += Vector3.down * (slideAmount - crouchAmount);
            if(!crouched) photonView.RPC("SetCrouch", RpcTarget.All, true);
        }

        //CameraField of view
        if (sliding) 
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, FOV * runFOVModifier * 1.15f, Time.deltaTime * 8f);
            normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin_cam + Vector3.down * slideAmount, Time.deltaTime * 6f);
        }
        else
        {
            if (isRunning)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, FOV * runFOVModifier, Time.deltaTime * 8f);
            }
            else
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, FOV, Time.deltaTime * 8f);
            }

            if (crouched)
            {
                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin_cam + Vector3.down * crouchAmount, Time.deltaTime * 6f);
            }
            else
            {
                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin_cam, Time.deltaTime * 6f);
            }
        }
        

        if (isSlope)
        {
            rb.AddForce(slopeMoveDirection.normalized * adjustedSpeed, ForceMode.Acceleration);
        }

        slopeMoveDirection = Vector3.ProjectOnPlane(dir, slopeHit.normal);
    }

    void ShakeGun(float ag_shake, float X_multiplier, float Y_multiplier)
    {
        float aim_adjust = 1f;
        if (weapon.isAiming) aim_adjust = 0.1f;
        targetShakePosition = weaponParentCurrentPos + new Vector3(Mathf.Cos(ag_shake) * X_multiplier * aim_adjust, Mathf.Sin(ag_shake * 2f) * Y_multiplier * aim_adjust, 0f); 
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public void TakeDamage(int dmg)
    {
        if (photonView.IsMine)
        {
            current_health -= dmg;
            RefreshHealBar();

            if(current_health <= 0)
            {
                manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    private void RefreshHealBar()
    {
        float health_ratio = (float)current_health / (float)max_health;
        healthbar_UI.localScale = Vector3.Lerp(healthbar_UI.localScale, new Vector3(health_ratio, 1, 1), Time.deltaTime * 8f);
    }

    [PunRPC]
    private void SetCrouch(bool c_state)
    {
        if (crouched == c_state) return;

        crouched = c_state;

        if (crouched)
        {
            standingCollider.SetActive(false);
            crouchingCollider.SetActive(true);
            weaponParentCurrentPos += Vector3.down * crouchAmount;
        }
        else
        {
            standingCollider.SetActive(true);
            crouchingCollider.SetActive(false);
            weaponParentCurrentPos -= Vector3.down * crouchAmount;
        }
    }

    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
    {
        if (p_stream.IsWriting)
        {
            p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
        }
        else
        {
            aimAngle = (int)p_stream.ReceiveNext() / 100f;
        }
    }

    private void RefreshMultiplayerState()
    {
        float EulerAngleY = weaponParent.localEulerAngles.y;

        Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
        weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

        Vector3 finalRotation = weaponParent.localEulerAngles;
        finalRotation.y = EulerAngleY;

        weaponParent.localEulerAngles = finalRotation;
    }
}
