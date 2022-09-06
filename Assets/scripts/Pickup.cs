using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;

    public bool isPickedUp = false;
    public bool canBePickedUp = true;

    public float onGroundScale;
    public float onHandsScale;

    [SerializeField] private float nextPickupTime = 0f;
    [SerializeField] private float throwPickupDelay = 1f;

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
        nextPickupTime = Time.time + throwPickupDelay;

        rb.isKinematic = false;

        gameObject.transform.localScale = Vector3.one * onGroundScale;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void Start()
    {
        OnDropped();
    }

    private void Update()
    {
        if (Time.time >= nextPickupTime && !isPickedUp)
            canBePickedUp = true;
    }
}