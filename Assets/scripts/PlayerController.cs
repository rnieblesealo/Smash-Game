using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private AnimationController anim;

    [SerializeField]private Transform groundChecker;

    [Header("Keybinds")]
    public KeyCode jump;
    public KeyCode shoot;
    public KeyCode reload;

    [Header("Settings")]
    public float maxJumpMultiplier = 1;
    public float jumpDistribution = 0.5f; // Proportion of jump allocated to upward movement; rest is for move direction boost
    public float jumpHoldIncrement = 0.05f;
    public float groundScanRadius;
    public float movementSpeed;
    public float jumpHeight;
    public float gravity;
    public float smoothTime;

    [Header("References")]
    public Transform model;

    public LayerMask whatIsGround;

    [HideInInspector] public float buttonX = 0;
    [HideInInspector] public float lastButtonX = 0;

    public bool isHoldingGun = true;
    public bool isGrounded = true;

    private float yVelocity;
    private float xVelocity;
    private float jumpMultiplier = 0;
    private bool hasJumped = false;

    private bool maxedJumpTimer = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<AnimationController>();
    }

    private void Move()
    {
        // Handle sidescroll movement
        buttonX = Input.GetAxisRaw("Horizontal");

        controller.Move(buttonX * movementSpeed * Time.deltaTime * Vector3.forward);

        // Store last pressed directional button that != 0
        if (lastButtonX != buttonX && buttonX != 0)
            lastButtonX = buttonX;

        // Handle player rotation
        if (model != null)
        {
            Vector3 rotation = Vector3.up * (lastButtonX < 0 ? 180 : 0);
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(rotation), smoothTime * Time.deltaTime);
        }
    }

    private void Fall()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, groundScanRadius, whatIsGround);

        if (isGrounded)
        {
            if (yVelocity < 0)
            {
                yVelocity = 0;
                xVelocity = 0;

                jumpMultiplier = 0;
                maxedJumpTimer = false;
                hasJumped = false;

                anim.PlaySound(1); // Play landing sound
            }
        }

        else
            yVelocity += gravity * Time.deltaTime;

        controller.Move(Time.deltaTime * yVelocity * Vector3.up); // Apply jump force
        controller.Move(Time.deltaTime * buttonX * xVelocity * Vector3.forward); // Apply additional horizontal force
    }

    private void Jump()
    {
        /*
         * Detect jump key if jump timer is not maxed, we must be grounded
         * Begin counting while jump key is pressed, we will be grounded during this
         * If jump key is released or jump multiplier is maxed, execute jump, we will not be grounded after this
         * Reset everything once we hit ground again
         */

        if (Input.GetKey(jump) && isGrounded && !maxedJumpTimer)
        {
            jumpMultiplier += jumpHoldIncrement;

            if (jumpMultiplier >= maxJumpMultiplier)
                maxedJumpTimer = true;
        }

        else if (Input.GetKeyUp(jump)  || maxedJumpTimer)
        {
            if (isGrounded && !hasJumped)
            {
                yVelocity = jumpDistribution * jumpHeight * jumpMultiplier;
                xVelocity = (1 - jumpDistribution) * jumpMultiplier * jumpHeight;

                anim.PlaySound(0); // Play jump sound

                hasJumped = true; // Ensures everything within this if statement executes only once per jump
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Fall();
        Jump();
    }
}
