using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask whatIsHittable;
    [SerializeField] private float hitRadius;
    [SerializeField] private float killTime = 1.5f;
    [SerializeField] private float speed = 0;
    [SerializeField] private int damage;

    public Transform caster;

    public Vector3 origin = Vector3.zero;
    public Vector3 target = Vector3.zero;

    private float lifetime = 0;

    public void Configure(Vector3 origin, Vector3 target, Transform caster)
    {
        this.origin = origin;
        this.target = target;
        this.caster = caster;
    }

    private void Update()
    {
        // Count up towards lifetime, if it is reached without hitting anything, bullet dies
        lifetime += Time.deltaTime;
        if (lifetime >= killTime)
            Destroy(gameObject);

        rb.velocity = speed * Time.deltaTime * (target - origin).normalized; // We must update this every frame for correct framerate independence, send values from gun

        // If a player is nearby, check for their controller and damage them, destroying this bullet
        bool playerNearby = Physics.CheckSphere(transform.position, hitRadius, whatIsHittable);
        if (playerNearby)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, hitRadius, whatIsHittable);
            for (int i = 0; i < nearbyColliders.Length; ++i)
            {
                if (nearbyColliders[i].transform != caster)
                {
                    nearbyColliders[i].GetComponent<PlayerController>().currentHealth -= damage;
                    Destroy(gameObject);
                }
            }
        }
    }
}
