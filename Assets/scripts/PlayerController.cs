using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public AnimationController anim;
    [HideInInspector] public Gun gun;

    [SerializeField] private Transform groundChecker;

    [Header("Keybinds")]
    public string buttonAxis;
    public KeyCode jump;
    public KeyCode shoot;
    public KeyCode reload;

    [Header("Settings")]
    public float throwTorque;
    public float throwForce;
    public float upThrowDistribution; // The higher this value, the more upward the object throw will go as opposed to sideways
    public float maxJumpMultiplier = 1;
    public float jumpDistribution = 0.5f; // Proportion of jump allocated to upward movement; rest is for move direction boost
    public float jumpHoldIncrement = 0.05f;
    public float groundScanRadius;
    public float itemScanRadius;
    public float movementSpeed;
    public float jumpHeight;
    public float gravity;
    public float smoothTime;

    [Header("References")]
    public Transform playerModel;
    public Transform heldPickupTransform;

    public LayerMask whatIsGround;
    public LayerMask whatIsPickup;

    [HideInInspector] public float buttonX = 0;
    [HideInInspector] public float lastButtonX = 0;

    public bool isHoldingGun = true;
    public bool isHoldingPickup = false; // TODO Redundant with heldPickup == null
    public bool isGrounded = true;

    private float yVelocity;
    private float xVelocity;
    private float jumpMultiplier = 0;

    private bool hasJumped = false;
    private bool maxedJumpTimer = false;

    private GameObject heldPickup;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<AnimationController>();
        gun = GetComponentInChildren<Gun>();
    }

    private void Move()
    {
        // Handle sidescroll movement
        buttonX = Input.GetAxisRaw(buttonAxis);

        controller.Move(buttonX * movementSpeed * Time.deltaTime * Vector3.forward);

        // Store last pressed directional button that != 0
        if (lastButtonX != buttonX && buttonX != 0)
            lastButtonX = buttonX;

        // Handle player rotation
        if (playerModel != null)
        {
            Vector3 rotation = Vector3.up * (lastButtonX < 0 ? 180 : 0);
            playerModel.transform.localRotation = Quaternion.Lerp(playerModel.transform.localRotation, Quaternion.Euler(rotation), smoothTime * Time.deltaTime);
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

    private bool IsTouchingPickup()
    {
        return Physics.CheckSphere(transform.position, itemScanRadius, whatIsPickup);
    }

    private void EquipPickup()
    {
        Collider[] nearbyPickups = Physics.OverlapSphere(transform.position, itemScanRadius, whatIsPickup);
        for (int i = 0; i < nearbyPickups.Length; ++i)
            if (nearbyPickups[i].GetComponent<Pickup>().canBePickedUp)
                heldPickup = nearbyPickups[i].gameObject;
        if (heldPickup == null)
            return;

        HolsterGun();

        heldPickup.transform.SetParent(heldPickupTransform);
        heldPickup.transform.localPosition = Vector3.zero;
        heldPickup.transform.localRotation = Quaternion.identity;

        heldPickup.GetComponent<Pickup>().OnPickedUp();

        isHoldingPickup = true;
    }

    private void ThrowPickup()
    {
        heldPickup.transform.SetParent(null);
        heldPickup.GetComponent<Pickup>().OnDropped();

        Rigidbody pickupRigidbody = heldPickup.GetComponent<Rigidbody>();

        Vector3 force = pickupRigidbody.mass * throwForce * (playerModel.transform.up * upThrowDistribution + playerModel.transform.forward * (1 - upThrowDistribution));
        Vector3 angularForce = throwTorque * (playerModel.transform.up + playerModel.transform.forward); // Results in a vector diagonal to the forward direction of the throw NOTE Only works sometimes?

        pickupRigidbody.AddForce(force, ForceMode.Impulse);
        pickupRigidbody.AddTorque(angularForce, ForceMode.Impulse);

        heldPickup = null;
        isHoldingPickup = false;

        DrawGun();
    }

    private void DrawGun()
    {
        if (!gun)
            return;

        gun.Draw();
        gun.nextFireTime = Time.time + 0.5f; // Delay gun firing a little so that we dont immediately shoot after switching actions

        isHoldingGun = true;
    }

    private void HolsterGun()
    {
        if (!gun)
            return;

        gun.Holster();
        isHoldingGun = false;
    }

    private void Start()
    {
        DrawGun();            
    }

    private void Update()
    {
        Move();
        Fall();
        Jump();

        if (IsTouchingPickup() && !isHoldingPickup)
            EquipPickup();

        if (Input.GetKeyDown(shoot) && isHoldingPickup)
            ThrowPickup();

        // Handle gun
        if (gun)
        {
            if (Input.GetKey(shoot))
                gun.Shoot();
            else if (Input.GetKeyDown(reload))
                gun.Reload();
        }
        else
            isHoldingGun = false;
    }
}
