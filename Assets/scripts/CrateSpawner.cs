using UnityEngine;

public class CrateSpawner : MonoBehaviour
{
    public GameObject spawnPrefab;
    public float spawnHeight;
    public float spawnInterval;
    public float junkDetectionRadius;
    public LayerMask whatIsJunk;
    public bool locked;

    private float nextSpawnTime;

    private void Spawn()
    {
        Instantiate(spawnPrefab, new Vector3(transform.position.x, transform.position.y + spawnHeight, transform.position.z), Quaternion.identity, null);
        nextSpawnTime += spawnInterval;
    }

    void Start()
    {
        nextSpawnTime = spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        locked = Physics.CheckSphere(transform.position, junkDetectionRadius, whatIsJunk);

        // While locked (something is on top of the spawner), disallow spawning; spawn once unlocked!
        if (!locked && Time.time >= nextSpawnTime)
            Spawn();
    }
}
