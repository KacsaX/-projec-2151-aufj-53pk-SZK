using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public bool godMode = false;
    private PlayerMovementNewInput movement;
    public Slider healthBar;

    private Animator animator;

    // szimplan readonly tulajdonság a halott állapot lekérdezéséhez más rendszerek számára, pl GameMaster)
    public bool IsDead { get; private set; } = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar == null)
            healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();

        movement = GetComponent<PlayerMovementNewInput>();
        if (healthBar != null)
            healthBar.maxValue = maxHealth;

        UpdateHealthBar();

        animator = GetComponent<Animator>();

        var gm = FindObjectOfType<GameMaster>();
        if (gm != null) gm.RegisterPlayer(this);
    }

    // sebzés logika
    public void TakeDamage(int amount)
    {
        if (godMode) return;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        // Játékos ütésének vizuális és gameplay hatásai
        var movement = GetComponent<PlayerMovementNewInput>();
        if (movement != null)
        {
            movement.AddLookOffset(-45f, 50f);
            movement.TriggerDrunkEffect(40f, 3f); // Drunk effect
        }

        CameraShake camShake = FindObjectOfType<CameraShake>();
        if (camShake != null)
            camShake.Shake();
    
        // Hit animáció felsőtesten
        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
            if (IsDead) return;
            IsDead = true;
            if (movement != null)
                movement.Die();

            // Mutatja a "You Died" képernyőt
            var deathUI = FindObjectOfType<DeathScreenController>();
            if (deathUI != null)
                deathUI.ShowDeathScreen();

            // Értesíti a GameMastert a halálról, hogy kezelhesse a respawn-t vagy egyéb logikát (Továbbfejlesztéshez hasznos lehet)
            var gm = FindObjectOfType<GameMaster>();
            if (gm != null) gm.NotifyPlayerDied(this);
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = currentHealth;
    }

    void OnDestroy()
    {
        // unregister from GameMaster if destroyed
        var gm = FindObjectOfType<GameMaster>();
        if (gm != null) gm.UnregisterPlayer(this);
    }
}