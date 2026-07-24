using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private string enemyName = "Enemy";
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private bool isFlying;

    [Header("Events")]
    public UnityEvent<int> onHealthChanged;   
    public UnityEvent<int> onDamaged;
    public UnityEvent<int> onVulnerableChanged;
    public UnityEvent onAttack;               
    public UnityEvent onDied;

    public string EnemyName => enemyName;
    public int Health { get; private set; }
    public int AttackDamage => attackDamage;
    public bool IsFlying => isFlying;
    public bool IsAlive => Health > 0;
    public int VulnerableStacks { get; private set; }

    private void Awake()
    {
        Health = maxHealth;
    }

    private void Start()
    {
        onHealthChanged.Invoke(Health);
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"Enemy {enemyName} took {amount} damage");

        if (!IsAlive || amount <= 0) return;

        Health = Mathf.Max(0, Health - amount);
        onDamaged.Invoke(amount);
        onHealthChanged.Invoke(Health);

        if (Health == 0)
        {
            onDied.Invoke();
            gameObject.SetActive(false);
        }
    }

    public void ApplyVulnerable(int stacks)
    {
        if (!IsAlive || stacks <= 0) return;
        VulnerableStacks += stacks;
        onVulnerableChanged.Invoke(VulnerableStacks);
        Debug.Log($"Enemy {enemyName} gained {stacks} vulnerable and now has {VulnerableStacks} vulnerable.");
    }

    public void TickVulnerable()
    {
        if (VulnerableStacks <= 0) return;
        VulnerableStacks--;
        onVulnerableChanged.Invoke(VulnerableStacks);
        Debug.Log($"Enemy {enemyName} lost a vulnerable and now has {VulnerableStacks}.");
    }

    public void NotifyAttacking()
    {
        onAttack.Invoke();
    }
}
