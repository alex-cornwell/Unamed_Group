using UnityEngine;

public class BentoItem : MonoBehaviour
{
    public float attractRadius = 8f;
    public float disappearDelay = 3f;
    public GameObject eatEffectPrefab;

    private bool isDropped = false;
    private bool isBeingEaten = false;
    private bool isPickedUp = false;

    public bool IsDropped => isDropped;
    public bool IsBeingEaten => isBeingEaten;
    public bool IsPickedUp => isPickedUp;

    public void DropOnMap(Vector3 worldPosition)
    {
        isDropped = true;
        transform.position = worldPosition;
        GetComponent<Collider2D>().enabled = false;
        Invoke(nameof(EnablePickup), 0.8f);
    }

    private void EnablePickup()
    {
        if (!isBeingEaten && !isPickedUp)
            GetComponent<Collider2D>().enabled = true;
    }

    public bool MarkAsPickedUp()
    {
        if (isPickedUp) return false;
        isPickedUp = true;
        GetComponent<Collider2D>().enabled = false;
        return true;
    }

    public void StartBeingEaten()
    {
        if (isBeingEaten) return;
        isBeingEaten = true;
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(EatRoutine());
    }

    private System.Collections.IEnumerator EatRoutine()
    {
        yield return new WaitForSeconds(disappearDelay);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}