using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.AddCoins(value);
        Sfx.Play("coin");
        Destroy(gameObject);
    }
}
