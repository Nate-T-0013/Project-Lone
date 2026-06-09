using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }
    public State currentState;
    public int damage = 1;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("References")]
    public Transform player;

    [Header("Movement")]
    public float speed = 2f;
    public float patrolDistance = 3f;

    [Header("Detection")]
    public float chaseRange = 5f;
    public float attackRange = 1.5f;

    private Vector2 startPos;
    private bool movingRight = true;

    void Awake()
    {
        //changed to automatically find player
        player = GameObject.Find("Player").GetComponent<Transform>();
        currentState = State.Patrol;
        startPos = transform.position;
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol(distance);
                break;

            case State.Chase:
                Chase(distance);
                break;

            case State.Attack:
                Attack(distance);
                break;
        }
    }

    void Patrol(float distance)
    {
        float direction = movingRight ? 1 : -1;
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, startPos) > patrolDistance)
        {
            movingRight = !movingRight;
        }

        if (distance < chaseRange)
        {
            currentState = State.Chase;
        }
    }

    void Chase(float distance)
    {
        Vector2 dir = (player.position - transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);

        if (distance < attackRange)
        {
            currentState = State.Attack;
        }
        else if (distance > chaseRange)
        {
            currentState = State.Patrol;
        }
    }

    

    void Attack(float distance)
    {

        if (distance <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Health playerHealth = player.GetComponent<Health>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log("Enemy attacking");
                }

                lastAttackTime = Time.time;
            }
        }

        if (distance > attackRange)
        {
            currentState = State.Chase;
        }
    }
    

        void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Something touched enemy");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player detected!");

            Health playerHealth = collision.gameObject.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                Debug.Log("No Health script found!");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger detected something!");

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player hit!");

            Health playerHealth = collision.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
