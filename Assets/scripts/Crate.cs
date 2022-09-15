using UnityEngine;

public class Crate : MonoBehaviour, IDamageable
{
    public GameObject contents;
    public int maxHealth;

    private int currentHealth;

    public void Damage(int amount)
    {
        currentHealth -= amount;
    }

    private void Break()
    {
        Instantiate(contents, transform.position, Quaternion.identity, null);
        Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            Break();
        }
    }
}
