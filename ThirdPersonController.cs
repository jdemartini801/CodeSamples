// Basic third person controller made for a project

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{

    // Components/objects
    public CharacterController cc;
    public Transform body, groundCheck;
    public Camera playerCamera;
    public Animator anim;

    // Values
    public LayerMask groundLayer;
    public float movementSpeed;
    public float rotationSpeed;
    public float jumpHeight;
    public float gravity;

    private bool isWalking, isSprinting, isGrounded;
    private float velocityY;

    // Update is called once per frame
    void Update()
    {

        bool paused = (Cursor.lockState == CursorLockMode.None);

        isGrounded = Physics.CheckSphere(groundCheck.position, .1f, groundLayer);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;

        Vector3 right = playerCamera.transform.right;

        Vector3 direction = (forward * vertical) + (right * horizontal);

        if(!paused)
        cc.Move(direction * movementSpeed * (isSprinting ? 2.5f : 1) * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && !paused)
        {
            if (isGrounded)
            {
                Jump();
            }
        }

        // Gravity
        cc.Move(new Vector3(0, velocityY * Time.deltaTime, 0));
        if (!isGrounded)
        {
            velocityY -= gravity * Time.deltaTime;
        }

        // Animator values
        isWalking = (horizontal != 0 || vertical != 0);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSprinting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
        }

        // Override values if game is paused so it doesn't continue animations
        if (paused)
        {
            isWalking = false;
            isSprinting = false;
        }

        anim.SetBool("IsWalking", isWalking);
        anim.SetBool("IsSprinting", isSprinting && isWalking);

        // Make the player rotate to face the direction in which they are moving
        RotateToCamera(direction);

    }

    public void RotateToCamera(Vector3 dir)
    {

        if (isWalking)
        {
            body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
        }

    }

    public void Jump()
    {

        anim.SetTrigger("Jump");
        velocityY = Mathf.Sqrt(2 * gravity * jumpHeight);

    }


}
