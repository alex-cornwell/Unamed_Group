using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chaseRange = 5f; // how close player must be to start chasing
    Rigidbody2D rb;
    Transform target;
    Vector2 moveDirection;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {  
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseController.IsGamePaused) return;

        if (target)
        {
            float distance = Vector2.Distance(transform.position, target.position);

            if (distance <= chaseRange)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                moveDirection = direction;
            }
            else
            {
                moveDirection = Vector2.zero; // stop moving when player is out of range
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return; // safety check

        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero; 
            return;
        }

        rb.linearVelocity = new Vector2(moveDirection.x, moveDirection.y) * moveSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
