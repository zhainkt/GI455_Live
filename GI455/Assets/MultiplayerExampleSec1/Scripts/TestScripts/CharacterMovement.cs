using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public Camera cameraTarget;
    public Transform characterMesh;
    public float moveSpeed = 5;
    public float gravity = 20;

    public float distanceCamera = 2.5f;

    public float cameraSensitivity = 90.0f;

    public Transform rootCam;
    public Transform subRootCam;

    private CharacterController characterController;

    private Vector3 velocity;

    public void Start()
    {
        characterController = this.GetComponent<CharacterController>();
    }

    public void Update()
    {
        if(characterController.isGrounded)
        {
            velocity = Vector3.zero;
            velocity += rootCam.transform.forward * Input.GetAxis("Vertical");
            velocity += rootCam.transform.right * Input.GetAxis("Horizontal");
            velocity.Normalize();
            velocity *= moveSpeed;
        }

        velocity.y -= gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);

        cameraTarget.transform.localPosition = Vector3.forward * (-distanceCamera);
        rootCam.RotateAroundLocal(Vector3.up, Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime);

        Vector3 camRot = subRootCam.transform.localEulerAngles;
        camRot.x -= Input.GetAxis("Mouse Y") * (cameraSensitivity*10) * Time.deltaTime;

        //camRot.x = Mathf.Clamp(camRot.x, -50.0f, 80.0f);

        subRootCam.transform.localEulerAngles = camRot;

        characterMesh.transform.rotation = Quaternion.Slerp(characterMesh.transform.rotation, rootCam.rotation, 5.0f * Time.deltaTime);
    }
}
