using UnityEngine;

public class Crate : MonoBehaviour, IDamageable
{
    private AudioSource audioSource;
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;

    [SerializeField] private GameObject contents;
    [SerializeField] private int maxHealth;
    [SerializeField] private float destroyDelay;
    [SerializeField] private AudioClip hitSoundEffect;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private AudioClip destroySoundEffect;
    [SerializeField] private ParticleSystem destroyParticles;

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
}
