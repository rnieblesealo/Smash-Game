public interface IDamageable
{
    public int maxHealth { get; set; }
    public int currentHealth { get; set; }

    public void Damage(int amount);
}
