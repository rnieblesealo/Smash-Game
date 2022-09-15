using UnityEngine;

public class Crate : MonoBehaviour, IDamageable
{
    private AudioSource audioSource;
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;
    private Rigidbody rb;

    [SerializeField] private GameObject contents;
    [SerializeField] private int maxHealth;
    [SerializeField] private float destroyDelay;
    [SerializeField] private AudioClip hitSoundEffect;
    [SerializeField] private AudioClip destroySoundEffect;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private ParticleSystem destroyParticles;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlayer;

    private int currentHealth;
    private bool broken = false;

    int IDamageable.maxHealth { get { return maxHealth; } set { } }
    int IDamageable.currentHealth { get { return currentHealth; } set { } }

    public void Damage(int amount)
    {
        currentHealth -= amount;
        audioSource.PlayOneShot(hitSoundEffect);
        hitParticles.Play();
    }

    private void Break()
    {
        // Mark as broken
        broken = true;

        Instantiate(contents, transform.position, Quaternion.identity, null);

        // Prevent collisions, de-render
        boxCollider.enabled = false;
        meshRenderer.enabled = false;

        // Play effects
        audioSource.PlayOneShot(destroySoundEffect);
        destroyParticles.Play();

        Destroy(gameObject, destroyDelay);
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentHealth <= 0 && !broken)
            Break();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Make rb kinematic once ground is hit for efficiency and gamestate management
        if (Utils.IsInLayerMask(collision.gameObject, whatIsGround))
            rb.isKinematic = true;

        else if (Utils.IsInLayerMask(collision.gameObject, whatIsPlayer) && !rb.isKinematic)
        {
            // Kill players instantly if falling crate touches them and break the box!
            rb.isKinematic = true;

            PlayerController hitPlayer = collision.gameObject.GetComponent<PlayerController>();
            hitPlayer.Damage(hitPlayer.settings.maxHealth); 
            Break();
        }

    }
}
