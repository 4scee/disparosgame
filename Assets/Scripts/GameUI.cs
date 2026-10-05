using UnityEngine;
using UnityEngine.SceneManagement;

// Pausa (Esc), Game Over y Victoria
public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    public GameObject pausePanel, gameOverPanel, victoryPanel;
    bool ended;

    void Awake() { Instance = this; Time.timeScale = 1f; }
    void OnDestroy() { Time.timeScale = 1f; if (Instance == this) Instance = null; }

    void Update()
    {
        if (!ended && Input.GetKeyDown(KeyCode.Escape)) { if (pausePanel.activeSelf) Resume(); else Pause(); }
    }

    public void Pause() { pausePanel.SetActive(true); Time.timeScale = 0f; }
    public void Resume() { pausePanel.SetActive(false); Time.timeScale = 1f; }

    public void ShowGameOver() { if (ended) return; ended = true; gameOverPanel.SetActive(true); Sfx.Play("gameover"); Time.timeScale = 0f; }
    public void ShowVictory() { if (ended) return; ended = true; victoryPanel.SetActive(true); Sfx.Play("win"); Time.timeScale = 0f; }

    public void Restart() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void ToMenu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
}
