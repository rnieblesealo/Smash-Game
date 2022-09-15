using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public AnimationController anim;
    [HideInInspector] public Gun gun;

    private Renderer[] meshes; // Cache of all renderers pertaining to this player

    public PlayerKeybinds keybinds;
    public PlayerSettings settings;

    [Header("Required Object References")]
    public Transform playerModel;
    public Transform heldPickupTransform;
    public Transform groundChecker;
    public Transform UIAnchor;

    [HideInInspector] public float buttonX = 0;
    [HideInInspector] public float lastButtonX = 0;
    [HideInInspector] public int currentHealth = 0;

    [HideInInspector] public bool isHoldingGun = true;
    [HideInInspector] public bool isGrounded = true;
    [HideInInspector] public bool isDead;

    private const int groundLayer = 3; // TODO Relocate to static type
    private const string unphaseableTag = "NoPhase";

    // Instance-Based Members

    private float yVelocity;
    private float xVelocity;
    private float timeToRespawn = 0;

    private bool hasJumped = false;
    private bool isPhasing = false;
    private bool canPhase = true;

    [HideInInspector] public GameObject heldPickup;

    // Damageable interface

    int IDamageable.maxHealth { get { return settings.maxHealth; } set { } }
    int IDamageable.currentHealth { get { return currentHealth; } set { } }

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

        if (isGrounded && !isPhasing)
        {
            if (yVelocity < 0)
            {
                yVelocity = 0;
                xVelocity = 0;

                hasJumped = false;
                canPhase = !IsTouchingBaseplate(); // On landing, check if we are standing on the baseplate, from which we can't phase down

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
        if (Input.GetKeyDown(keybinds.jump))
        {
            if (isGrounded && !hasJumped)
            {
                yVelocity = settings.jumpDistribution * settings.jumpHeight;
                xVelocity = (1 - settings.jumpDistribution) * settings.jumpHeight;

                anim.PlaySound(0); // Play jump sound

                hasJumped = true; // Ensures everything within this if statement executes only once per jump
                canPhase = true; // If we jump we can phase always
            }
        }
    }

    private bool IsTouchingPickup()
    {
        return Physics.CheckSphere(transform.position, settings.itemScanRadius, settings.whatIsPickup);
    }

    private bool IsTouchingBaseplate()
    {
        // Check nearby colliders to see if we are standing on a baseplate
        Collider[] nearbyColliders = Physics.OverlapSphere(groundChecker.position, settings.groundScanRadius);
        for (int i = 0; i < nearbyColliders.Length; ++i)
            if (nearbyColliders[i].gameObject.CompareTag(unphaseableTag))
                return true;
        return false;
    }

    private void EquipPickup()
    {
        Collider[] nearbyPickups = Physics.OverlapSphere(transform.position, settings.itemScanRadius, settings.whatIsPickup);
        for (int i = 0; i < nearbyPickups.Length; ++i)
            if (nearbyPickups[i].GetComponent<Pickup>().canBePickedUp)
                heldPickup = nearbyPickups[i].gameObject;
        if (!heldPickup)
            return;

        HolsterGun();

        heldPickup.transform.SetParent(heldPickupTransform);
        heldPickup.transform.localPosition = Vector3.zero;
        heldPickup.transform.localRotation = Quaternion.identity;

        heldPickup.GetComponent<Pickup>().OnPickedUp();
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

    private void OnDeath(bool diedOffMap = false)
    {
        /* On death:
         * 
         * Hide renderers under this player, NOTE any renderers added after game start will not be disabled
         * Disallow any input (stop updating player)
         * Drop any pickups
         * Determine at what time we will respawn
         * Disable hitbox AND character controller hitbox
         * Stop any animation (disable animator altogether)
         * Make health bar transparent to reflect dead player, handle this from healthbar script itself
         * Disable walk particles, this is handled from animation controller
         * Play effects! Only if player did not die off-map.
         * Move player to default respawn point if died off map
         */

        // Disallow updates to this player, including input
        isDead = true;

        // Disable non-particle renderers
        for (int i = 0; i < meshes.Length; ++i)
            if (!meshes[i].GetComponent<ParticleSystem>())
                meshes[i].enabled = false;

        // Disable controller, this includes hitbox
        controller.enabled = false;

        // Disable animator
        anim.animator.enabled = false;

        // Drop all pickups if any are held
        if (heldPickup)
            ThrowPickup();

        // Move to default respawn point
        if (diedOffMap)
            transform.position = LevelController.safetyRespawnPoint;

        // Play effects if died on map!
        else
        {
            anim.deathParticles.Play();
            anim.PlaySound(4);
        }

        // Determine respawn time
        timeToRespawn = Time.time + settings.respawnDelay;
    }

    private void OnRespawn()
    {
        /*On respawn:
         *
         * Reset health
         * Reset animation
         * Reset gun(refill, reset state)
         * Reset player controller(clear all state)
         * Reenable hitboxes
         * Reenable renderers
         * Reenable animator
         * Play effects!
         * Make health bar full opacity again, handle this from healthbar script itself
         * Enable walk particles, this is handled from animator script itself
         */

        // Restore health
        currentHealth = settings.maxHealth;

        // Restore updates
        isDead = false;

        // Reset state bools & values
        yVelocity = 0;
        xVelocity = 0;

        hasJumped = false;
        isPhasing = false;

        // Re-enable non-particle renderers
        for (int i = 0; i < meshes.Length; ++i)
            meshes[i].enabled = true;

        // Re-enable hitboxes, remember that controller includes hitbox
        controller.enabled = true;

        // Enable animator
        anim.animator.enabled = true;

        // Reset gun
        gun.Reset();
        DrawGun();
    }

    public void Damage(int amount)
    {
        // One cannot damage a dead player
        if (isDead)
            return;

        currentHealth -= amount;

        anim.hitParticles.Play();
        anim.PlaySound(3); // 3 is the hitmarker sound
    }


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<AnimationController>();
        gun = GetComponentInChildren<Gun>();
        meshes = GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        currentHealth = settings.maxHealth;

        DrawGun();          
    }

    private void Update()
    {
        // Player will only respond to respawn update if dead
        if (isDead)
        {
            if (Time.time >= timeToRespawn)
                OnRespawn();
            return;
        }

        Move();
        Fall();
        Jump();

        if (IsTouchingPickup() && !heldPickup)
            EquipPickup();

        if (Input.GetKey(keybinds.shoot) && heldPickup)
            ThrowPickup();

        // Handle gun
        if (gun)
        {
            if (Input.GetKey(keybinds.shoot))
                gun.Shoot();
            else if (Input.GetKeyDown(keybinds.reload))
                gun.BeginReload();
        }
        else
            isHoldingGun = false;

        // Handle phasing
        if (canPhase)
        {
            // Handle platform phasing
            if (yVelocity < settings.jumpPhaseDeadzone || yVelocity < 0 && hasJumped)
                isPhasing = false;

            // Handle fall velocity by down input
            if (Input.GetKeyDown(keybinds.phaseDown) && isGrounded)
            {
                yVelocity = settings.downwardPhaseVelocity;
                isPhasing = true;
            }

            // Handle fall velocity by jumping
            else if (yVelocity > 0)
                isPhasing = true;
        }

        else
            isPhasing = false;
        
        // Handle death, clamp current health to 0 as its min value
        if (currentHealth <= 0 && !isDead || transform.position.y <= LevelController.killYLevel)
        {
            currentHealth = 0;
            OnDeath(diedOffMap: transform.position.y <= LevelController.killYLevel);
        }
    }

    private void LateUpdate()
    {
        // Set global ground collision state based on plastform phasing
        for (int i = 0; i < LevelController.levelObjects.Count; ++i)
            Physics.IgnoreCollision(controller, LevelController.levelObjects[i], isPhasing);
    }
}