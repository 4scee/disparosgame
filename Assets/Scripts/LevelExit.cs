using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    public int levelIndex = 1;
    public string nextScene = "Level2";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Sfx.Play("win");
        GameManager.Instance.CompleteLevel(levelIndex);
        SceneManager.LoadScene(nextScene);
    }
}
