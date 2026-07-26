using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private string enemyName = "Enemy";
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private bool isFlying;
    [SerializeField] private int stamina = -1;              // How many times they can attack before tiring (negative numbers can attack forever)
    [SerializeField] private bool canAttack = true;
    [SerializeField] private bool canFly = false;
    [SerializeField] private bool isImpervious = false;

    [Header("Events")]
    public UnityEvent<int> onHealthChanged;   
    public UnityEvent<int> onDamaged;
    public UnityEvent<bool> onVulnerableChanged;
    public UnityEvent<bool> onFlyingChanged;
    public UnityEvent<bool> onImpervousChanged;
    public UnityEvent onDamageNegated;
    public UnityEvent onExhausted;
    public UnityEvent onAttack;               
    public UnityEvent onDied;

    [Header("Animations")]
    [SerializeField] private Anim_EnemyAttack attackAnim;

    public string EnemyName => enemyName;
    public int Health { get; private set; }
    public int AttackDamage => attackDamage;
    public bool IsFlying => isFlying;
    public bool IsAlive => Health > 0;
    public bool IsImpervious => isImpervious;
    public int VulnerableStacks { get; private set; }
    public Anim_EnemyAttack AttackAnim => attackAnim;
    public bool CanAttack => canAttack;

    private void Awake()
    {
        Health = maxHealth;
        if (!canAttack) canAttack = true;
        if (canFly && !isFlying)
        {
            isFlying = true;
        }
    }

    private void Start()
    {
        onHealthChanged.Invoke(Health);
        onFlyingChanged.Invoke(isFlying);
        onImpervousChanged.Invoke(isImpervious);
    }

    public void TakeDamage(int amount)
    {
        if (isImpervious)
        {
            Debug.Log($"Enemy {enemyName} is impervious to damage");
            onDamageNegated.Invoke();
            return;
        }

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
        onVulnerableChanged.Invoke(VulnerableStacks > 0);
        UpdateImpervious();
        Debug.Log($"Enemy {enemyName} gained {stacks} vulnerable and now has {VulnerableStacks} vulnerable.");
    }

    public void TickVulnerable()
    {
        if (VulnerableStacks <= 0) return;
        VulnerableStacks--;
        onVulnerableChanged.Invoke(VulnerableStacks > 0);
        Debug.Log($"Enemy {enemyName} lost a vulnerable and now has {VulnerableStacks}.");
    }

    public void NotifyAttacking()
    {
        onAttack.Invoke();
    }

    public void AttackUpdates()
    {
        UpdateStamina();
        UpdateFlight();
    }

    private void UpdateStamina()
    {
        if (stamina <= -1) return;
        else
        {
            stamina--;
            if (stamina == 0)
            {
                canAttack = false;
                onExhausted.Invoke();
                ApplyVulnerable(99);
            }
        }
    }

    private void UpdateFlight()
    {
        if (canFly && isFlying)
        {
            isFlying = false;
            onFlyingChanged.Invoke(isFlying);
        }
        else if (canFly && !isFlying)
        {
            isFlying = true;
            onFlyingChanged.Invoke(isFlying);
        }
    }

    private void UpdateImpervious()
    {
        if (isImpervious && VulnerableStacks > 0) isImpervious = false;
        onImpervousChanged.Invoke(isImpervious);
    }
}
