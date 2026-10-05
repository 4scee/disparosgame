using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public int levelIndex = 1;
    public Text coinsText, healthText, levelText, bossText;
    PlayerHealth ph;
    Enemy boss;

    void Start()
    {
        GameManager.Instance.EnterLevel(levelIndex);
        Sfx.PlayMusic("music_level" + levelIndex);
        var p = GameObject.FindWithTag("Player");
        if (p) ph = p.GetComponent<PlayerHealth>();
        foreach (var e in FindObjectsOfType<Enemy>()) if (e.isBoss) boss = e;
    }

    void Update()
    {
        if (coinsText) coinsText.text = "Monedas: " + GameManager.Instance.Data.coins;
        if (levelText) levelText.text = "Nivel " + levelIndex;
        if (healthText && ph) healthText.text = "Vida: " + Mathf.Max(0, ph.Health) + " / " + ph.MaxHealth;
        if (bossText)
            bossText.text = (boss != null && Mathf.Abs(boss.transform.position.x - ph.transform.position.x) < 25f)
                ? "JEFE: " + boss.Health + " / " + boss.MaxHealth : "";
    }
}
