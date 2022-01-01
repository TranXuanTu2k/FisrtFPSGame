using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CamRotation : MonoBehaviourPunCallbacks
{

    public Transform player;
    public Transform cam;
    public float XSensivitive;
    public float YSensivitive;
    public float MaxAngle;
    public static bool CursorVisible = true;

    [SerializeField] WallRun wallRun;
    /*[SerializeField] private float sensX = 100f;
    [SerializeField] private float sensY = 100f;

    [SerializeField] Transform cam = null;
    [SerializeField] Transform orientation = null;

    float mouseX;
    float mouseY;

    float multiplier = 0.01f;

    float xRotation;
    float yRotation;*/

    private Quaternion camcenter;
    public GameObject camParent;

    void Start()
    {
        /*if (photonView.IsMine)
        {
            camParent.SetActive(true);
        }*/
        camParent.SetActive(photonView.IsMine);
        camcenter = cam.localRotation;
    }

    
    private void Update()
    {

        /*mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.rotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);*/
        if (!photonView.IsMine)
        {
            return;
        }
        setY();
        setX();
        UpdateCursorVisible();
    }
    
    void setY()
    {
        float c_input = Input.GetAxis("Mouse Y") * YSensivitive * Time.deltaTime;
        Quaternion quaternion = Quaternion.AngleAxis(c_input, Vector3.left);
        Quaternion c_delta = cam.localRotation * quaternion;

        if (Quaternion.Angle(camcenter, c_delta) < MaxAngle)
        {
            cam.localRotation = c_delta;
        }
    }

    void setX()
    {
        float c_input = Input.GetAxis("Mouse X") * XSensivitive * Time.deltaTime;
        Quaternion quaternion = Quaternion.AngleAxis(c_input, Vector3.up);
        Quaternion c_delta = player.localRotation * quaternion;
        
        player.localRotation = c_delta;
    }

    void UpdateCursorVisible()
    {
        if (CursorVisible)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CursorVisible = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CursorVisible = true;
            }
        }
    }
}
