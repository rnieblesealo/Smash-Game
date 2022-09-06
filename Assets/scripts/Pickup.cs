using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;

    public bool isPickedUp = false;
    public bool canBePickedUp = true;
    public bool isGrounded = false;
    public bool isDestroyed = false;

    public float onGroundScale;
    public float onHandsScale;

    [HideInInspector] public Transform lastOwner;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private LayerMask whatIsHittable;
    [SerializeField] private int groundLayer = 3;
    [SerializeField] private float hitRadius = 1f;
    [SerializeField] private float killTime = 1.5f;
    [SerializeField] private int damage;

    public void OnPickedUp()
    {
        isPickedUp = true;
        canBePickedUp = false;

        rb.isKinematic = true;

        gameObject.transform.localScale = Vector3.one * onHandsScale;
    }

    public void OnDropped()
    {
        isPickedUp = false;

        rb.isKinematic = false;

        gameObject.transform.localScale = Vector3.one * onGroundScale;
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
        OnDropped();
    }

    private void Update()
    {
        // If this object has been destroyed stop updating it
        if (isDestroyed)
            return;

        //If a player is nearby and the item cannot be picked up, we can check for any collisions
        if (!canBePickedUp)
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
        if (isPickedUp)
            return;

        if (collision.gameObject.layer == groundLayer)
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (isPickedUp)
            return;

        if (collision.gameObject.layer == groundLayer)
            isGrounded = false;
    }
}