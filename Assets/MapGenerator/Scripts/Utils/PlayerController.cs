using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public abstract class Command
    {
        public abstract void Execute();
    }

    public class JumpFunction : Command
    {
        public override void Execute()
        {
            Jump();
        }
    }

    public class TelekinesisFunction : Command
    {
        public override void Execute()
        {
            Telekinesis();
        }
    }

    public static void Telekinesis()
    {

    }

    public static void Jump()
    {
    }

    public static void DoMove()
    {
        Command keySpace = new JumpFunction();
        Command keyX = new TelekinesisFunction();

        if (Input.GetButton("Jump"))
        {
            keySpace.Execute();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            keyX.Execute();
        }
    }


    public CharacterController characterController;
    public float speed = 3;


    public Animator animator;

    private int JumpHash = Animator.StringToHash("Jump");
    private int SpeedHash = Animator.StringToHash("Speed");

    // camera and rotation
    public Transform cameraHolder;
    public float mouseSensitivity = 2f;
    public float upLimit = -50;
    public float downLimit = 50;
    public float jumpHeight = 10;

    // gravity
    private float gravity = -9.87f;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    void Update()
    {
        Move();
        Rotate();

    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Rotate()
    {
        float horizontalRotation = Input.GetAxis("CameraHorizontal");
        float verticalRotation = Input.GetAxis("CameraVertical");

        transform.Rotate(0, horizontalRotation * mouseSensitivity, 0);
        cameraHolder.Rotate(-verticalRotation * mouseSensitivity, 0, 0);
        Vector3 currentRotation = cameraHolder.localEulerAngles;
        if (currentRotation.x > 180) currentRotation.x -= 360;
        currentRotation.x = Mathf.Clamp(currentRotation.x, upLimit, downLimit);
        cameraHolder.localRotation = Quaternion.Euler(currentRotation);
    }

    public bool IsOnGroundRayCast()
    {
        RaycastHit hit;
        bool isHit = Physics.Raycast(characterController.bounds.center, Vector3.down, out hit, characterController.bounds.extents.y + 1f);
        return isHit;
    }

    private void Move()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");

        groundedPlayer = IsOnGroundRayCast();
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = transform.forward * verticalMove + transform.right * horizontalMove;
        characterController.Move(move * Time.deltaTime * speed);
        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }

        playerVelocity.y += gravity * Time.deltaTime;

        characterController.Move(playerVelocity * Time.deltaTime);
        animator.SetBool(JumpHash, !IsOnGroundRayCast());
        animator.SetFloat(SpeedHash, (speed * new Vector2(move.x, move.z)).sqrMagnitude * Mathf.Sign(move.z));
    }
}
