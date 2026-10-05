#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Menú: Tools > Metal Slug > Generar proyecto completo   /   Compilar (Build)
public static class MetalSlugSetup
{
    const int GroundLayer = 8;
    const float PPU = 16f;
    static readonly Dictionary<string, Sprite> spr = new Dictionary<string, Sprite>();
    static GameObject pBullet, pEnemyBullet, pCoin, pHealth, pExplosion, pEnemy, pBoss, pPlayer;
    static PhysicsMaterial2D noFriction;

    // ------------------------------------------------------------------ MAIN
    [MenuItem("Tools/Metal Slug/Generar proyecto completo")]
    public static void BuildAll()
    {
        foreach (var d in new[] { "Assets/Prefabs", "Assets/Animations", "Assets/Scenes" })
            if (!Directory.Exists(d)) Directory.CreateDirectory(d);
        AssetDatabase.Refresh();

        EnsureGroundLayer();
        ImportSprites();
        noFriction = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        AssetDatabase.DeleteAsset("Assets/Prefabs/NoFriction.physicsMaterial2D");
        AssetDatabase.CreateAsset(noFriction, "Assets/Prefabs/NoFriction.physicsMaterial2D");

        var ctrl = CreatePlayerController();
        BuildPrefabs(ctrl);

        BuildLevel(1);
        BuildLevel(2);
        BuildMenu();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2.unity", true),
        };
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("Proyecto generado. Presiona Play desde la escena MainMenu.");
    }

    [MenuItem("Tools/Metal Slug/Compilar (Build)")]
    public static void BuildGame()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        string ext = target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows ? ".exe"
                   : target == BuildTarget.StandaloneOSX ? ".app" : "";
        Directory.CreateDirectory("Builds");
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Level1.unity", "Assets/Scenes/Level2.unity" },
            locationPathName = "Builds/MetalSlug2D" + ext,
            target = target,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Build: " + report.summary.result + " -> " + opts.locationPathName);
        if (report.summary.result == BuildResult.Succeeded) EditorUtility.RevealInFinder(opts.locationPathName);
    }

    // ------------------------------------------------------------------ ASSETS
    static void EnsureGroundLayer()
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tm.FindProperty("layers").GetArrayElementAtIndex(GroundLayer).stringValue = "Ground";
        tm.ApplyModifiedProperties();
    }

    static void ImportSprites()
    {
        spr.Clear();
        foreach (var raw in Directory.GetFiles("Assets/Sprites", "*.png"))
        {
            string f = raw.Replace("\\", "/");
            AssetDatabase.ImportAsset(f);
            var imp = (TextureImporter)AssetImporter.GetAtPath(f);
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = PPU;
            imp.filterMode = FilterMode.Point;
            imp.mipmapEnabled = false;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            var ts = new TextureImporterSettings();
            imp.ReadTextureSettings(ts);
            ts.spriteMeshType = SpriteMeshType.FullRect;
            ts.spriteAlignment = (int)SpriteAlignment.Center;
            imp.SetTextureSettings(ts);
            imp.SaveAndReimport();
            spr[Path.GetFileNameWithoutExtension(f)] = AssetDatabase.LoadAssetAtPath<Sprite>(f);
        }
    }

    static Sprite S(string n) { return spr[n]; }
    static Sprite[] F(string prefix, int n)
    {
        var a = new Sprite[n];
        for (int i = 0; i < n; i++) a[i] = spr[prefix + i];
        return a;
    }

    static AnimationClip MakeClip(string name, Sprite[] frames, float fps, bool loop)
    {
        var clip = new AnimationClip { name = name, frameRate = fps };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var s = AnimationUtility.GetAnimationClipSettings(clip); s.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        string path = $"Assets/Animations/{name}.anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static AnimatorController CreatePlayerController()
    {
        string path = "Assets/Animations/Player.controller";
        AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Shoot", AnimatorControllerParameterType.Trigger);
        var sm = ctrl.layers[0].stateMachine;

        var idle = sm.AddState("Idle"); idle.motion = MakeClip("Idle", F("player_idle", 2), 3, true);
        var walk = sm.AddState("Walk"); walk.motion = MakeClip("Walk", F("player_walk", 4), 10, true);
        var jump = sm.AddState("Jump"); jump.motion = MakeClip("Jump", F("player_jump", 1), 1, false);
        var shoot = sm.AddState("Shoot"); shoot.motion = MakeClip("Shoot", F("player_shoot", 2), 12, false);
        sm.defaultState = idle;

        var t = idle.AddTransition(walk); t.hasExitTime = false; t.duration = 0; t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        t = walk.AddTransition(idle); t.hasExitTime = false; t.duration = 0; t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        t = sm.AddAnyStateTransition(jump); t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false; t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
        t = jump.AddTransition(idle); t.hasExitTime = false; t.duration = 0; t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        t = sm.AddAnyStateTransition(shoot); t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false; t.AddCondition(AnimatorConditionMode.If, 0, "Shoot");
        t = shoot.AddTransition(idle); t.hasExitTime = true; t.exitTime = 1f; t.duration = 0;
        return ctrl;
    }

    // ------------------------------------------------------------------ PREFABS
    static GameObject MakePrefab(string name, string sprite, int order, System.Action<GameObject> setup)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = S(sprite); sr.sortingOrder = order;
        setup(go);
        string path = $"Assets/Prefabs/{name}.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static Transform Child(GameObject parent, string name, Vector3 localPos)
    {
        var c = new GameObject(name); c.transform.SetParent(parent.transform, false);
        c.transform.localPosition = localPos; return c.transform;
    }

    static void BuildPrefabs(AnimatorController ctrl)
    {
        pBullet = MakePrefab("Bullet", "bullet", 8, go =>
        {
            var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(0.5f, 0.19f); c.isTrigger = true;
            go.AddComponent<Bullet>();
        });
        pEnemyBullet = MakePrefab("EnemyBullet", "enemy_bullet", 8, go =>
        {
            var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0;
            var c = go.AddComponent<CircleCollider2D>(); c.radius = 0.19f; c.isTrigger = true;
            go.AddComponent<EnemyBullet>();
        });
        pCoin = MakePrefab("Coin", "coin0", 4, go =>
        {
            var c = go.AddComponent<CircleCollider2D>(); c.radius = 0.38f; c.isTrigger = true;
            var a = go.AddComponent<SpriteAnimator>(); a.frames = F("coin", 4); a.fps = 8;
            go.AddComponent<Coin>();
        });
        pHealth = MakePrefab("HealthPickup", "health", 4, go =>
        {
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(0.75f, 0.75f); c.isTrigger = true;
            go.AddComponent<HealthPickup>();
        });
        pExplosion = MakePrefab("Explosion", "explosion0", 20, go =>
        {
            var a = go.AddComponent<SpriteAnimator>(); a.frames = F("explosion", 4); a.fps = 12; a.loop = false; a.destroyAtEnd = true;
        });
        pEnemy = MakePrefab("Enemy", "enemy_walk0", 6, go =>
        {
            go.AddComponent<Rigidbody2D>().gravityScale = 3;
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(0.8f, 1.9f); c.offset = new Vector2(0, -0.05f); c.sharedMaterial = noFriction;
            var a = go.AddComponent<SpriteAnimator>(); a.frames = F("enemy_walk", 2); a.fps = 5;
            var e = go.AddComponent<Enemy>(); e.health = 3; e.drops = new[] { pCoin, pHealth }; e.deathEffect = pExplosion;
            var sh = go.AddComponent<EnemyShooter>(); sh.bulletPrefab = pEnemyBullet; sh.firePoint = Child(go, "FirePoint", new Vector3(0.7f, 0.12f, 0));
        });
        pBoss = MakePrefab("Boss", "boss0", 6, go =>
        {
            go.AddComponent<Rigidbody2D>().gravityScale = 3;
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(3.8f, 2.3f); c.offset = new Vector2(0, -0.1f); c.sharedMaterial = noFriction;
            var a = go.AddComponent<SpriteAnimator>(); a.frames = new[] { S("boss0"), S("boss1") }; a.fps = 4;
            var e = go.AddComponent<Enemy>();
            e.health = 30; e.speed = 1.2f; e.sightRange = 16f; e.stopDistance = 6f; e.isBoss = true; e.deathEffect = pExplosion; e.dropChance = 0;
            var sh = go.AddComponent<EnemyShooter>();
            sh.bulletPrefab = pEnemyBullet; sh.firePoint = Child(go, "FirePoint", new Vector3(2.0f, 0.6f, 0));
            sh.aimAtPlayer = true; sh.bulletCount = 3; sh.spreadAngle = 14f; sh.fireRate = 1.4f; sh.range = 18f;
        });
        pPlayer = MakePrefab("Player", "player_idle0", 10, go =>
        {
            go.tag = "Player";
            var rb = go.AddComponent<Rigidbody2D>();
            var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(0.8f, 1.9f); c.offset = new Vector2(0, -0.05f); c.sharedMaterial = noFriction;
            var anim = go.AddComponent<Animator>(); anim.runtimeAnimatorController = ctrl;
            var pc = go.AddComponent<PlayerController>();
            var sh = go.AddComponent<PlayerShooter>();
            go.AddComponent<PlayerHealth>();
            pc.groundCheck = Child(go, "GroundCheck", new Vector3(0, -1.0f, 0));
            pc.groundLayer = 1 << GroundLayer;
            sh.firePoint = Child(go, "FirePoint", new Vector3(0.7f, 0.12f, 0));
            sh.bulletPrefab = pBullet;
        });
    }

    // ------------------------------------------------------------------ NIVELES
    static GameObject Solid(string name, Vector2 center, Vector2 size, string sprite, Color tint, int order, bool oneWay)
    {
        var go = new GameObject(name) { layer = GroundLayer };
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = S(sprite); sr.drawMode = SpriteDrawMode.Tiled; sr.size = size; sr.color = tint; sr.sortingOrder = order;
        go.transform.position = center;
        var col = go.AddComponent<BoxCollider2D>(); col.size = size; col.sharedMaterial = noFriction;
        if (oneWay) { col.usedByEffector = true; var eff = go.AddComponent<PlatformEffector2D>(); eff.useOneWay = true; eff.surfaceArc = 160f; eff.useSideFriction = false; }
        return go;
    }

    static void Ground(float x0, float x1, Color tint)
    {
        float w = x1 - x0;
        Solid("Ground", new Vector2((x0 + x1) / 2f, -4f), new Vector2(w, 4f), "dirt", tint, 0, false);
        var g = new GameObject("Grass");
        var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = S("grass"); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(w, 0.375f); sr.color = tint; sr.sortingOrder = 1;
        g.transform.position = new Vector2((x0 + x1) / 2f, -2f + 0.1f);
    }

    static void Platform(float x, float top, float w)
    {
        Solid("Platform", new Vector2(x, top - 0.25f), new Vector2(w, 0.5f), "platform", Color.white, 1, true);
    }

    static void Wall(float x)
    {
        var go = new GameObject("Wall") { layer = GroundLayer };
        go.transform.position = new Vector2(x, 5);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1, 30);
    }

    static void Spawn(GameObject prefab, float x, float y)
    {
        var o = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        o.transform.position = new Vector3(x, y, 0);
    }

    static void BgLayer(string sprite, float factor, float yOff, int order, Color tint)
    {
        var go = new GameObject("BG_" + sprite);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = S(sprite); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(500, sr.sprite.rect.height / PPU);
        sr.sortingOrder = order; sr.color = tint;
        var p = go.AddComponent<ParallaxLayer>(); p.factorX = factor; p.yOffset = yOff;
    }

    static void BuildLevel(int idx)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        bool night = idx == 2;
        Color tint = night ? new Color(0.65f, 0.6f, 0.85f) : Color.white;

        var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGO.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 5.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = night ? new Color(0.12f, 0.1f, 0.24f) : new Color(0.5f, 0.77f, 0.95f);
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 0.5f, -10);
        var cf = camGO.AddComponent<CameraFollow>();
        cf.minX = 0f; cf.minY = -0.5f; cf.maxY = 8f; cf.maxX = idx == 1 ? 98f : 128f;

        BgLayer("clouds", 0.1f, 3.5f, -30, night ? new Color(0.5f, 0.45f, 0.7f, 0.8f) : Color.white);
        BgLayer("hills_far", 0.25f, -3.0f, -20, tint);
        BgLayer("ruins_near", 0.5f, -3.2f, -10, tint);

        var pl = (GameObject)PrefabUtility.InstantiatePrefab(pPlayer);
        pl.transform.position = new Vector3(0, -0.5f, 0); cf.target = pl.transform;

        Wall(-14f);
        float exitX;
        if (idx == 1)
        {
            Ground(-12, 32, tint); Ground(36, 62, tint); Ground(66, 112, tint);
            foreach (var p in new[] { (10f, 0.5f, 4f), (18f, 1.5f, 4f), (26f, 0.5f, 3f), (44f, 0.5f, 5f), (52f, 1.5f, 3f), (75f, 0.5f, 4f), (84f, 1.5f, 4f), (93f, 0.5f, 4f) })
                Platform(p.Item1, p.Item2, p.Item3);
            foreach (var x in new[] { 16f, 24f, 40f, 48f, 56f, 70f, 80f, 90f, 98f }) Spawn(pEnemy, x, -0.5f);
            foreach (var x in new[] { 5f, 6f, 7f, 40f, 41f, 58f, 59f, 78f, 79f, 100f }) Spawn(pCoin, x, -1.2f);
            foreach (var c in new[] { (10f, 1.5f), (18f, 2.5f), (26f, 1.5f), (34f, 1.2f), (44f, 1.5f), (52f, 2.5f), (64f, 1.2f), (75f, 1.5f), (84f, 2.5f), (93f, 1.5f) }) Spawn(pCoin, c.Item1, c.Item2);
            Spawn(pHealth, 28f, -1.2f); Spawn(pHealth, 60f, -1.2f); Spawn(pHealth, 86f, 2.4f);
            exitX = 106f; Wall(113f);
        }
        else
        {
            Ground(-12, 28, tint); Ground(32, 56, tint); Ground(60, 84, tint); Ground(88, 150, tint);
            foreach (var p in new[] { (8f, 0.5f, 4f), (16f, 1.5f, 4f), (22f, 0.5f, 3f), (40f, 0.5f, 4f), (46f, 1.5f, 3f), (68f, 0.5f, 5f), (76f, 1.5f, 3f), (96f, 0.5f, 4f) })
                Platform(p.Item1, p.Item2, p.Item3);
            foreach (var x in new[] { 12f, 20f, 26f, 36f, 44f, 52f, 64f, 72f, 80f, 92f, 98f, 104f, 108f, 112f }) Spawn(pEnemy, x, -0.5f);
            foreach (var x in new[] { 4f, 5f, 36f, 37f, 64f, 65f, 92f, 93f, 100f }) Spawn(pCoin, x, -1.2f);
            foreach (var c in new[] { (8f, 1.5f), (16f, 2.5f), (22f, 1.5f), (30f, 1.2f), (40f, 1.5f), (46f, 2.5f), (58f, 1.2f), (68f, 1.5f), (76f, 2.5f), (86f, 1.2f), (96f, 1.5f) }) Spawn(pCoin, c.Item1, c.Item2);
            Spawn(pHealth, 18f, -1.2f); Spawn(pHealth, 50f, -1.2f); Spawn(pHealth, 90f, -1.2f); Spawn(pHealth, 118f, -1.2f);
            Spawn(pBoss, 130f, -0.5f);
            exitX = -999f; Wall(148f);
        }

        if (exitX > -900f)
        {
            var exit = new GameObject("LevelExit"); exit.transform.position = new Vector3(exitX, -0.5f, 0);
            var sr = exit.AddComponent<SpriteRenderer>(); sr.sprite = S("platform"); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(1.5f, 3f);
            sr.color = new Color(0.2f, 1f, 0.3f, 0.6f); sr.sortingOrder = 2;
            exit.AddComponent<BoxCollider2D>().isTrigger = true;
            var le = exit.AddComponent<LevelExit>(); le.levelIndex = idx; le.nextScene = "Level2";
            exit.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 3f);
        }

        // ---- UI
        var canvas = MakeCanvas();
        var hud = canvas.AddComponent<HUD>(); hud.levelIndex = idx;
        hud.coinsText = TopLeft(canvas.transform, "Coins", new Vector2(20, -15));
        hud.healthText = TopLeft(canvas.transform, "Health", new Vector2(20, -55));
        hud.levelText = TopLeft(canvas.transform, "Level", new Vector2(20, -95));
        hud.bossText = Centered(canvas.transform, "Boss", "", 34, new Vector2(0, 300), 500);

        var ui = canvas.AddComponent<GameUI>();
        ui.pausePanel = MakePanel(canvas.transform, "Pausa", "PAUSA");
        MakeButton(ui.pausePanel.transform, "Continuar", new Vector2(0, 20), ui.Resume);
        MakeButton(ui.pausePanel.transform, "Reiniciar nivel", new Vector2(0, -60), ui.Restart);
        MakeButton(ui.pausePanel.transform, "Menú principal", new Vector2(0, -140), ui.ToMenu);
        ui.gameOverPanel = MakePanel(canvas.transform, "GameOver", "GAME OVER");
        MakeButton(ui.gameOverPanel.transform, "Reintentar", new Vector2(0, 20), ui.Restart);
        MakeButton(ui.gameOverPanel.transform, "Menú principal", new Vector2(0, -60), ui.ToMenu);
        ui.victoryPanel = MakePanel(canvas.transform, "Victoria", "¡VICTORIA!");
        Centered(ui.victoryPanel.transform, "Sub", "Derrotaste al jefe. ¡Misión cumplida!", 30, new Vector2(0, 100), 800);
        MakeButton(ui.victoryPanel.transform, "Menú principal", new Vector2(0, -20), ui.ToMenu);
        ui.pausePanel.SetActive(false); ui.gameOverPanel.SetActive(false); ui.victoryPanel.SetActive(false);
        MakeEventSystem();

        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/Level{idx}.unity");
    }

    static void BuildMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGO.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.08f, 0.1f, 0.18f);
        camGO.AddComponent<AudioListener>();

        var canvas = MakeCanvas();
        var menu = canvas.AddComponent<MainMenu>();
        Picture(canvas.transform, "player_idle0", new Vector2(-480, -60), 8);
        Picture(canvas.transform, "boss0", new Vector2(450, -90), 5);
        Centered(canvas.transform, "Title", "METAL SLUG 2D", 72, new Vector2(0, 250), 900);
        menu.statusText = Centered(canvas.transform, "Status", "", 26, new Vector2(0, 160), 900);
        menu.playButton = MakeButton(canvas.transform, "Jugar / Continuar", new Vector2(0, 60), menu.Play);
        menu.newGameButton = MakeButton(canvas.transform, "Nueva partida", new Vector2(0, -20), menu.NewGame);
        menu.quitButton = MakeButton(canvas.transform, "Salir", new Vector2(0, -100), menu.Quit);
        Centered(canvas.transform, "Help", "← → mover   ·   Espacio saltar   ·   J / Ctrl disparar   ·   Esc pausa", 24, new Vector2(0, -230), 1000);
        MakeEventSystem();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    // ------------------------------------------------------------------ UI helpers
    static GameObject MakeCanvas()
    {
        var go = new GameObject("Canvas");
        var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.AddComponent<CanvasScaler>(); cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; cs.referenceResolution = new Vector2(1280, 720);
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static void MakeEventSystem() { new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); }

    static Text NewText(Transform parent, string name, string txt, int size, TextAnchor align)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = txt; t.fontSize = size; t.alignment = align; t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        var o = go.AddComponent<Outline>(); o.effectColor = Color.black;
        return t;
    }

    static Text TopLeft(Transform parent, string name, Vector2 pos)
    {
        var t = NewText(parent, name, "", 30, TextAnchor.UpperLeft);
        var rt = t.rectTransform; rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.sizeDelta = new Vector2(500, 40); rt.anchoredPosition = pos;
        return t;
    }

    static Text Centered(Transform parent, string name, string txt, int size, Vector2 pos, float width)
    {
        var t = NewText(parent, name, txt, size, TextAnchor.MiddleCenter);
        var rt = t.rectTransform; rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, 80); rt.anchoredPosition = pos;
        return t;
    }

    static void Picture(Transform parent, string sprite, Vector2 pos, float scale)
    {
        var go = new GameObject("Pic_" + sprite); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.sprite = S(sprite); img.preserveAspect = true; img.SetNativeSize();
        go.transform.localScale = Vector3.one * scale;
        go.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    static GameObject MakePanel(Transform parent, string name, string title)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = new Color(0, 0, 0, 0.75f);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Centered(go.transform, "Title", title, 72, new Vector2(0, 200), 900);
        return go;
    }

    static Button MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = new Color(0.2f, 0.28f, 0.45f);
        var b = go.AddComponent<Button>(); b.targetGraphic = img;
        var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(340, 60); rt.anchoredPosition = pos;
        var t = Centered(go.transform, "Label", label, 28, Vector2.zero, 340);
        t.rectTransform.sizeDelta = new Vector2(340, 60);
        UnityEventTools.AddPersistentListener(b.onClick, action);
        return b;
    }
}
#endif
