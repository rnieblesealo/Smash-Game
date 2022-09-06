using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public AnimationController anim;
    [HideInInspector] public Gun gun;

    public PlayerKeybinds keybinds;
    public PlayerSettings settings;

    [Header("Required Object References")]
    public Transform playerModel;
    public Transform heldPickupTransform;
    public Transform groundChecker;
    public Transform UIAnchor;

    [HideInInspector] public float buttonX = 0;
    [HideInInspector] public float lastButtonX = 0;
    [HideInInspector] public float currentHealth = 0;

    [HideInInspector] public bool isHoldingGun = true;
    [HideInInspector] public bool isHoldingPickup = false; // TODO Redundant with heldPickup == null
    [HideInInspector] public bool isGrounded = true;

    private const int groundLayer = 3; // TODO Relocate to static type

    // Instance-Based Members

    private float yVelocity;
    private float xVelocity;
    private float currentJumpMultiplier = 0;

    private bool hasJumped = false;
    private bool maxedJumpTimer = false;
    private bool ignoringGroundCollision = false;

    private GameObject heldPickup;

    // Type Functions

    private void Move()
    {
        // Handle sidescroll movement
        buttonX = Input.GetAxisRaw(keybinds.buttonAxis);

        controller.Move(buttonX * settings.movementSpeed * Time.deltaTime * Vector3.forward);

        // Store last pressed directional button that != 0
        if (lastButtonX != buttonX && buttonX != 0)
            lastButtonX = buttonX;

        // Handle player rotation
        if (playerModel != null)
        {
            Vector3 rotation = Vector3.up * (lastButtonX < 0 ? 180 : 0);
            playerModel.transform.localRotation = Quaternion.Lerp(playerModel.transform.localRotation, Quaternion.Euler(rotation), settings.smoothTime * Time.deltaTime);
        }
    }

    private void Fall()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, settings.groundScanRadius, settings.whatIsGround);

        if (isGrounded)
        {
            if (yVelocity < 0)
            {
                yVelocity = 0;
                xVelocity = 0;

                currentJumpMultiplier = 0;
                maxedJumpTimer = false;
                hasJumped = false;

                anim.PlaySound(1); // Play landing sound
            }
        }

        else
            yVelocity += settings.gravity * Time.deltaTime;

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

        if (Input.GetKey(keybinds.jump) && isGrounded && !maxedJumpTimer)
        {
            currentJumpMultiplier += settings.jumpHoldIncrement;

            if (currentJumpMultiplier >= settings.maxJumpMultiplier)
                maxedJumpTimer = true;
        }

        else if (Input.GetKeyUp(keybinds.jump)  || maxedJumpTimer)
        {
            if (isGrounded && !hasJumped)
            {
                yVelocity = settings.jumpDistribution * settings.jumpHeight * currentJumpMultiplier;
                xVelocity = (1 - settings.jumpDistribution) * currentJumpMultiplier * settings.jumpHeight;

                anim.PlaySound(0); // Play jump sound

                hasJumped = true; // Ensures everything within this if statement executes only once per jump
            }
        }
    }

    private bool IsTouchingPickup()
    {
        return Physics.CheckSphere(transform.position, settings.itemScanRadius, settings.whatIsPickup);
    }

    private void EquipPickup()
    {
        Collider[] nearbyPickups = Physics.OverlapSphere(transform.position, settings.itemScanRadius, settings.whatIsPickup);
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
        heldPickup.GetComponent<Pickup>().lastOwner = this.transform; // The last of
    }

    private void ThrowPickup()
    {
        /* On throw:
         * Use similar collision system as bullets
         */

        heldPickup.transform.SetParent(null);
        heldPickup.GetComponent<Pickup>().OnDropped();

        Rigidbody pickupRigidbody = heldPickup.GetComponent<Rigidbody>();

        Vector3 force = pickupRigidbody.mass * settings.throwForce * (playerModel.transform.up * settings.upThrowDistribution + playerModel.transform.forward * (1 - settings.upThrowDistribution));
        Vector3 angularForce = settings.throwTorque * playerModel.transform.forward; // Results in a vector diagonal to the forward direction of the throw NOTE Only works sometimes?

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

    public void Damage(int amount)
    {
        currentHealth -= amount;

        anim.bloodParticles.Play();
        anim.PlaySound(3); // 3 is the hitmarker sound
    }

    // MonoBehavior Functions

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<AnimationController>();
        gun = GetComponentInChildren<Gun>();
    }

    private void Start()
    {
        currentHealth = settings.maxHealth;

        DrawGun();          
    }

    private void Update()
    {
        Move();
        Fall();
        Jump();

        if (IsTouchingPickup() && !isHoldingPickup)
            EquipPickup();

        if (Input.GetKeyDown(keybinds.shoot) && isHoldingPickup)
            ThrowPickup();

        // Handle gun
        if (gun)
        {
            if (Input.GetKey(keybinds.shoot))
                gun.Shoot();
            else if (Input.GetKeyDown(keybinds.reload))
                gun.Reload();
        }
        else
            isHoldingGun = false;

        // Handle platform phasing
        if (yVelocity > 0 && !ignoringGroundCollision)
        {
            for (int i = 0; i < LevelController.levelObjects.Count; ++i)
                Physics.IgnoreCollision(controller, LevelController.levelObjects[i], true);

            ignoringGroundCollision = true;
        }

        else if (yVelocity <= 0 && ignoringGroundCollision)
        {
            for (int i = 0; i < LevelController.levelObjects.Count; ++i)
                Physics.IgnoreCollision(controller, LevelController.levelObjects[i], false);

            ignoringGroundCollision = false;
        }
    }
}