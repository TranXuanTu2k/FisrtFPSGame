using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SwayGun : MonoBehaviour
{

    [Header("SwaySetting")]
    [SerializeField] private float Smooth;
    [SerializeField] private float swayMutiplier;
    public bool isMine;

    private Quaternion origin_rot;

    void Start()
    {
        origin_rot = transform.localRotation;
    }

    void Update()
    {
        Sway();
    }

    void Sway()
    {
        //get mouse input
        float x_mouse = Input.GetAxis("Mouse X") * swayMutiplier;
        float y_mouse = Input.GetAxis("Mouse Y") * swayMutiplier;

        if (!isMine)
        {
            x_mouse = 0;
            y_mouse = 0;
        }

        //calculate target rotation
        Quaternion XRotation = Quaternion.AngleAxis(-x_mouse, Vector3.up);
        Quaternion YRotation = Quaternion.AngleAxis(y_mouse, Vector3.right);

        Quaternion target_rotation = origin_rot * XRotation * YRotation;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, target_rotation, Smooth * Time.deltaTime);
    }
}
