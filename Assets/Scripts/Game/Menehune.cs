using System.Collections;
using UnityEngine;

public class Menehune : MonoBehaviour
{
    public enum MenehuneState { Patrol, Chase, GoToBento, Eating, Disappearing }

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseRange = 5f;
    public float eatRange = 0.4f;           // how close to bento before eating

    [Header("Bento Detection")]
    public float bentoCheckInterval = 0.5f; // how often to scan for bento

    [Header("Effects")]
    public GameObject disappearEffectPrefab;
    public float disappearDuration = 1f;

    private MenehuneState state = MenehuneState.Patrol;
    private Rigidbody2D rb;
    private Animator animator;
    private Transform playerTarget;
    private BentoItem targetBento;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;

        StartCoroutine(BentoScanner());
    }

    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            moveDirection = Vector2.zero;
            return;
        }

        if (playerTarget != null && state != MenehuneState.GoToBento 
            && state != MenehuneState.Eating && state != MenehuneState.Disappearing)
        {
            float dist = Vector2.Distance(transform.position, playerTarget.position);
            if (dist <= chaseRange)
                state = MenehuneState.Chase;
            else if (state == MenehuneState.Chase)
            {
                state = MenehuneState.Patrol;
                moveDirection = Vector2.zero;
            }
        }


        switch (state)
        {
            case MenehuneState.Chase:
                HandleChase();
                break;
            case MenehuneState.GoToBento:
                HandleGoToBento();
                break;
            case MenehuneState.Eating:
            case MenehuneState.Disappearing:
                moveDirection = Vector2.zero;
                break;
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection * moveSpeed;
    }

    // -------------------------------------------------------------------------
    // State Handlers
    // -------------------------------------------------------------------------

    private void HandleChase()
    {
        if (playerTarget == null) return;

        float dist = Vector2.Distance(transform.position, playerTarget.position);
        if (dist <= chaseRange)
        {
            moveDirection = (playerTarget.position - transform.position).normalized;
        }
        else
        {
            moveDirection = Vector2.zero;
            state = MenehuneState.Patrol;
        }
    }

    private void HandleGoToBento()
    {
        if (targetBento == null || !targetBento.IsDropped)
        {
            state = MenehuneState.Patrol;
            return;
        }

        float dist = Vector2.Distance(transform.position, targetBento.transform.position);

        if (dist <= eatRange)
        {
            StartCoroutine(EatBento());
        }
        else
        {
            moveDirection = (targetBento.transform.position - transform.position).normalized;
        }
    }

    // -------------------------------------------------------------------------
    // Bento Logic
    // -------------------------------------------------------------------------

    // Periodically scans for dropped bento in range
    private IEnumerator BentoScanner()
    {
        while (true)
        {
            yield return new WaitForSeconds(bentoCheckInterval);

            if (state == MenehuneState.Eating || state == MenehuneState.Disappearing)
                continue;

            BentoItem[] bentos = FindObjectsByType<BentoItem>(FindObjectsSortMode.None);
            BentoItem closest = null;
            float closestDist = float.MaxValue;

            foreach (BentoItem bento in bentos)
            {
                if (!bento.IsDropped || bento.IsBeingEaten) continue;

                float dist = Vector2.Distance(transform.position, bento.transform.position);
                if (dist < bento.attractRadius && dist < closestDist)
                {
                    closest = bento;
                    closestDist = dist;
                }
            }

            if (closest != null)
            {
                targetBento = closest;
                state = MenehuneState.GoToBento;
                moveSpeed *= 1.5f; // speed up when going for bento
            }
        }
    }

    private IEnumerator EatBento()
    {
        state = MenehuneState.Eating;
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        // Tell bento it's being eaten
        targetBento.StartBeingEaten();

        // Wait a moment then disappear
        yield return new WaitForSeconds(1.5f);

        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        state = MenehuneState.Disappearing;

        if (disappearEffectPrefab != null)
            Instantiate(disappearEffectPrefab, transform.position, Quaternion.identity);

        // Fade out
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float elapsed = 0f;
            Color startColor = sr.color;
            while (elapsed < disappearDuration)
            {
                elapsed += Time.deltaTime;
                sr.color = Color.Lerp(startColor, Color.clear, elapsed / disappearDuration);
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Animator
    // -------------------------------------------------------------------------

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetBool("IsWalking", moveDirection.magnitude > 0);
        if (moveDirection.magnitude > 0)
        {
            animator.SetFloat("InputX", moveDirection.x);
            animator.SetFloat("InputY", moveDirection.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
