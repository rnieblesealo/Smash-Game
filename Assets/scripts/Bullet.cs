using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float killTime = 1.5f;
    [SerializeField] private float speed = 0;
    
    public Vector3 origin = Vector3.zero;
    public Vector3 target = Vector3.zero;

    private float lifetime = 0;

    private void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= killTime)
            Destroy(gameObject);

        rb.velocity = speed * Time.deltaTime * (target - origin); // We must update this every frame for correct framerate independence, send values from gun
    }
}
