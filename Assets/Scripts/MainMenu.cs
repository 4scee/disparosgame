using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 3.9 menú de inicio + botón Salir
public class MainMenu : MonoBehaviour
{
    public Button playButton, newGameButton, quitButton;
    public Text statusText;

    void Start() { Sfx.PlayMusic("music_menu"); }

    void Update()
    {
        var gm = GameManager.Instance;
        bool ready = gm.Loaded;
        if (playButton) playButton.interactable = ready;
        if (newGameButton) newGameButton.interactable = ready;
        if (statusText)
            statusText.text = !ready ? "Cargando progreso..." :
                $"Monedas: {gm.Data.coins}   Nivel 1: {(gm.Data.level1Done ? "OK" : "-")}   Nivel 2: {(gm.Data.level2Done ? "OK" : "-")}";
    }

    public void Play() { SceneManager.LoadScene("Level" + Mathf.Clamp(GameManager.Instance.Data.lastLevel, 1, 2)); }

    public void NewGame() { GameManager.Instance.ResetProgress(); SceneManager.LoadScene("Level1"); }

    public void Quit()
    {
        GameManager.Instance.SaveNow();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
