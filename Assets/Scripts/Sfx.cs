using System.Collections.Generic;
using UnityEngine;

// Audio sencillo: carga clips desde Resources/Audio
public static class Sfx
{
    static AudioSource sfx, music;
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    static void Init()
    {
        if (sfx != null && music != null) return;
        var go = new GameObject("AudioManager");
        UnityEngine.Object.DontDestroyOnLoad(go);
        sfx = go.AddComponent<AudioSource>();
        music = go.AddComponent<AudioSource>();
        music.loop = true; music.volume = 0.35f;
    }

    static AudioClip Get(string n)
    {
        if (!cache.TryGetValue(n, out var c) || c == null) { c = Resources.Load<AudioClip>("Audio/" + n); cache[n] = c; }
        return c;
    }

    public static void Play(string n, float vol = 0.6f) { Init(); var c = Get(n); if (c != null) sfx.PlayOneShot(c, vol); }

    public static void PlayMusic(string n)
    {
        Init(); var c = Get(n);
        if (c == null || (music.clip == c && music.isPlaying)) return;
        music.clip = c; music.Play();
    }
}
