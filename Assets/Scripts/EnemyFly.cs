using System.Collections;
using UnityEngine;

public class EnemyFly : EnemyBase
{
    [Header("Hareket ve Saldırı")]
    public float moveSpeed = 2f;
    public float patrolRadius = 3f;
    public float aggroRange = 6f;
    public float attackRange = 3f;
    
    [Header("Dash Ayarları")]
    public float dashForce = 10f;
    public float attackCooldown = 2f;
    public float attackRecoveryDelay = 0.5f; //Saldırı sonrası bekleme süresi

    private Transform player;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float lastAttackTime;

    private enum State { Patrol, Chase, Attack, Stun, Dead }
    private State currentState;

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
        currentState = State.Patrol;
        SetNewPatrolPoint();
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isDead) return;

        switch (currentState)
        {
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Chase:
                HandleChase();
                break;
            // Attack ve Stun durumlarında özel bir update işlemi yok
        }

        if (currentState != State.Attack && currentState != State.Stun)
        {
            FlipSprite();
        }
    }

    // --- LOGIC ---

    void HandlePatrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            SetNewPatrolPoint();

        if (player != null && Vector2.Distance(transform.position, player.position) < aggroRange)
        {
            currentState = State.Chase;
        }
    }

    void HandleChase()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * 1.5f * Time.deltaTime);
        }

        if (dist > aggroRange * 1.5f)
        {
            currentState = State.Patrol;
        }
    }

    IEnumerator AttackRoutine()
    {
        currentState = State.Attack;
        rb.linearVelocity = Vector2.zero;
        FlipSprite(); 
        yield return new WaitForSeconds(0.2f); 
        Vector2 dir = (player.position - transform.position).normalized;
        anim.SetTrigger("Attack");
        rb.AddForce(dir * dashForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.5f);
        rb.linearVelocity = Vector2.zero; 
        yield return new WaitForSeconds(attackRecoveryDelay);
        lastAttackTime = Time.time;
        currentState = State.Chase;
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        if (!isDead && currentState != State.Dead)
        {
            StopAllCoroutines();
            StartCoroutine(StunRoutine());
        }
    }

    IEnumerator StunRoutine()
    {
        currentState = State.Stun;
        rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(0.5f);

        if (!isDead) currentState = State.Chase;
    }

    void SetNewPatrolPoint()
    {
        targetPosition = startPosition + (Vector2)Random.insideUnitCircle * patrolRadius;
    }

    void FlipSprite()
    {
        if (isDead || player == null) return;

        Vector3 target = (currentState == State.Chase || currentState == State.Attack) 
                         ? player.position 
                         : (Vector3)targetPosition;

        if (target.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }
        else if (target.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);  
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}