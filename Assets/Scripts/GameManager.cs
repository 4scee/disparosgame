using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int coins;
    public bool level1Done;
    public bool level2Done;
    public int lastLevel = 1;
}

// 3.8 estado global del juego (monedas y avance de 2 niveles)
public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("GameManager");
                _instance = go.AddComponent<GameManager>();
            }
            return _instance;
        }
    }

    public SaveData Data = new SaveData();
    public bool Loaded { get; private set; }
    DatabaseService db;

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        db = gameObject.AddComponent<DatabaseService>();
        db.Load(d => { Data = d; Loaded = true; });
    }

    public void AddCoins(int n) { Data.coins += n; SaveNow(); }

    public void EnterLevel(int level) { Data.lastLevel = level; }

    public void CompleteLevel(int level)
    {
        if (level == 1) { Data.level1Done = true; Data.lastLevel = 2; }
        if (level == 2) { Data.level2Done = true; Data.lastLevel = 2; }
        SaveNow();
    }

    public void ResetProgress() { Data = new SaveData(); SaveNow(); }

    public void SaveNow() { if (db != null && Loaded) db.Save(Data); }

    void OnApplicationQuit() { SaveNow(); }
}
