using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int amount = 1;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var h = other.GetComponent<PlayerHealth>();
        if (h) h.Heal(amount);
        Sfx.Play("pickup");
        Destroy(gameObject);
    }
}
