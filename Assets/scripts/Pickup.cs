using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class Pickup : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;

    [Header("Status")]
    public bool isPickedUp = false;
    public bool canBePickedUp = true;
    public bool isGrounded = false;
    public bool isDestroyed = false;

    [Header("Settings")]
    public float onGroundScale;
    public float onHandsScale;
    public float onHandsRotation;

    public bool isWeapon;
    public int maxAmmo;

    [HideInInspector] public Transform lastOwner;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private LayerMask whatIsHittable;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float hitRadius = 1f;
    [SerializeField] private float killTime = 1.5f;
    [SerializeField] private int damage;

    private bool isVirgin = true; // Has this object been picked up at least once?
    private int currentAmmo;

    public void OnPickedUp()
    {
        if (isVirgin)
            isVirgin = false;

        isPickedUp = true;
        canBePickedUp = false;

        rb.isKinematic = true;

        gameObject.transform.localScale = Vector3.one * onHandsScale;
    }

    public void OnThrown()
    {
        isPickedUp = false;
        rb.isKinematic = false;

        gameObject.transform.localScale = Vector3.one * onGroundScale;
    }

    // By default, using a pickup throws it
    public virtual void OnUsed()
    {
        print("Using pickup!");
    }

    private void OnDestroyed(PlayerController target = null)
    {
        isDestroyed = true;

        meshRenderer.enabled = false;
        rb.isKinematic = true;
        col.enabled = false;

        // Stop all particles that aren't explosion particles
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystems.Length; ++i)
            if (particleSystems[i] != explosionParticles)
                particleSystems[i].Stop();

        // Damage target if one was passed
        if (target)
            target.Damage(damage);

        // Play effects
        explosionParticles.Play();
        audioSource.PlayOneShot(explosionSound);

        Destroy(gameObject, killTime);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        OnThrown();

        // Makes object always collectable w/o start collision
        isVirgin = true;
        isGrounded = true;
    }

    private void Update()
    {
        // If this object has been destroyed stop updating it
        if (isDestroyed)
            return;

        //If a player is nearby and the item cannot be picked up, we can check for any collisions
        if (!isPickedUp)
        {
            {
                bool playerNearby = Physics.CheckSphere(transform.position, hitRadius, whatIsHittable);
                if (playerNearby)
                {
                    Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, hitRadius, whatIsHittable);
                    for (int i = 0; i < nearbyColliders.Length; ++i)
                    {
                        // The player this is colliding effectively with can never be the last caster nor null
                        if (nearbyColliders[i].transform != lastOwner && lastOwner != null)
                        {
                            OnDestroyed(nearbyColliders[i].GetComponent<PlayerController>());
                        }
                    }
                }
            }
        }

        // Object can never be grounded if picked up
        if (isPickedUp && isGrounded)
            isGrounded = false;

        // Can be picked up if no one is holding it and is grounded
        canBePickedUp = !isPickedUp && isGrounded;
    }

    // Object is grounded if collider is touching ground
    private void OnCollisionEnter(Collision collision)
    {
        if (isPickedUp || isGrounded || isVirgin)
            return;

        if (Utils.IsInLayerMask(collision.gameObject, whatIsGround))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (isPickedUp || isGrounded || isVirgin)
            return;

        if (Utils.IsInLayerMask(collision.gameObject, whatIsGround))
            isGrounded = false;
    }
}