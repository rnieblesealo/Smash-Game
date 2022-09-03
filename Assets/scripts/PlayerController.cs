using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private AnimationController anim;

    [SerializeField]private Transform groundChecker;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jump;

    [Header("Settings")]
    public float maxJumpMultiplier = 1;
    public float jumpDistribution = 0.5f; // Proportion of jump allocated to upward movement; rest is for move direction boost
    public float jumpHoldIncrement = 0.05f;
    public float groundScanRadius;
    public float movementSpeed;
    public float jumpHeight;
    public float gravity;
    public float smoothTime;

    public LayerMask whatIsGround;

    [HideInInspector] public float buttonX;

    public bool isGrounded = true;

    private float yVelocity;
    private float xVelocity;
    private float lastButtonX = 0;
    private float jumpMultiplier = 0;

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
        Vector3 rotation = Vector3.up * (lastButtonX < 0 ? 180 : 0);
        gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, Quaternion.Euler(rotation), smoothTime * Time.deltaTime);
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
            }
        }

        else
            yVelocity += gravity;

        controller.Move(Vector3.up * yVelocity * Time.deltaTime); // Apply jump force
        controller.Move(Vector3.forward * xVelocity * buttonX * Time.deltaTime); // Apply additional horizontal force
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
            if (isGrounded)
            {
                yVelocity = jumpDistribution * jumpHeight * jumpMultiplier;
                xVelocity = (1 - jumpDistribution) * jumpMultiplier * jumpHeight;
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
