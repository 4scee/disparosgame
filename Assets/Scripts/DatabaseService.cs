using System;
using System.Collections.Generic;
using UnityEngine;
#if FIREBASE_FIRESTORE
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
#endif

// 3.8 Conexión a Firebase Cloud Firestore.
// Si el SDK de Firebase NO está instalado, usa PlayerPrefs como respaldo para que el proyecto compile.
// Para activar Firestore: instala FirebaseFirestore.unitypackage y agrega el símbolo
// FIREBASE_FIRESTORE en Project Settings > Player > Scripting Define Symbols.
public class DatabaseService : MonoBehaviour
{
    const string LocalKey = "metalslug_save";
    const string Collection = "players";
    static string PlayerId => SystemInfo.deviceUniqueIdentifier;

#if FIREBASE_FIRESTORE
    FirebaseFirestore db;
#endif

    SaveData LoadLocal()
    {
        string json = PlayerPrefs.GetString(LocalKey, "");
        return string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
    }

    void SaveLocal(SaveData d)
    {
        PlayerPrefs.SetString(LocalKey, JsonUtility.ToJson(d));
        PlayerPrefs.Save();
    }

    public void Load(Action<SaveData> done)
    {
#if FIREBASE_FIRESTORE
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(t =>
        {
            if (t.Result != DependencyStatus.Available)
            {
                Debug.LogWarning("Firebase no disponible: " + t.Result);
                done(LoadLocal());
                return;
            }
            db = FirebaseFirestore.DefaultInstance;
            db.Collection(Collection).Document(PlayerId).GetSnapshotAsync().ContinueWithOnMainThread(s =>
            {
                if (s.IsFaulted || s.IsCanceled || !s.Result.Exists) { done(LoadLocal()); return; }
                var dict = s.Result.ToDictionary();
                var d = new SaveData
                {
                    coins = GetInt(dict, "coins", 0),
                    level1Done = GetBool(dict, "level1Done"),
                    level2Done = GetBool(dict, "level2Done"),
                    lastLevel = GetInt(dict, "lastLevel", 1)
                };
                SaveLocal(d);
                Debug.Log("Progreso cargado desde Firestore");
                done(d);
            });
        });
#else
        done(LoadLocal());
#endif
    }

    public void Save(SaveData d)
    {
        SaveLocal(d);
#if FIREBASE_FIRESTORE
        if (db == null) return;
        var dict = new Dictionary<string, object>
        {
            { "coins", d.coins },
            { "level1Done", d.level1Done },
            { "level2Done", d.level2Done },
            { "lastLevel", d.lastLevel }
        };
        db.Collection(Collection).Document(PlayerId).SetAsync(dict).ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted) Debug.LogError("Error al guardar: " + t.Exception);
            else Debug.Log("Progreso guardado en Firestore");
        });
#endif
    }

    static int GetInt(Dictionary<string, object> d, string k, int def)
        => d.TryGetValue(k, out var v) ? Convert.ToInt32(v) : def;
    static bool GetBool(Dictionary<string, object> d, string k)
        => d.TryGetValue(k, out var v) && Convert.ToBoolean(v);
}
