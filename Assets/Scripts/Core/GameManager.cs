// GameManager.cs — safe spawn + auto-wire Grid/Input + snap to grid
using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using UInput = UnityEngine.Input;

using LostPlanet.GridSystem;
using LostPlanet.Managers;
using LostPlanet.UI;

namespace LostPlanet.Core
{
    public enum GameState { Boot, MainMenu, LoadingLevel, Playing, Paused, LevelComplete, GameOver }

    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public GameState State { get; private set; } = GameState.Boot;

        [Header("Managers (Assign in scene or by AutoTestSceneBootstrap)")]
        public GridManager GridManager;
        public CrystalManager CrystalManager;
        public LifeManager LifeManager;
        public SaveManager SaveManager;
        public UIManager UIManager;
        public AudioManager AudioManager;

        [Header("Prefabs")]
        public GameObject PlayerPrefab;

        [Header("Level")]
        public int CurrentLevelId = 1;
        public Transform LevelRoot;

        GameObject _player;
        UIFader _fader;
        bool _isPaused;

        void Start()
        {
            if (UIManager == null || GridManager == null)
            {
                Debug.Log("[GameManager] Waiting for bootstrap to call Init()/StartLevel...");
                return;
            }
        }

        public void Init()
        {
            if (!UIManager) UIManager = FindObjectOfType<UIManager>();
            if (!SaveManager) SaveManager = FindObjectOfType<SaveManager>();
            if (!LifeManager) LifeManager = FindObjectOfType<LifeManager>();
            if (!CrystalManager) CrystalManager = FindObjectOfType<CrystalManager>();
            if (!GridManager) GridManager = FindObjectOfType<GridManager>();
            if (!AudioManager) AudioManager = FindObjectOfType<AudioManager>();

            SaveManager?.Init();
            AudioManager?.Init();

            if (_fader == null)
            {
                _fader = FindObjectOfType<UIFader>();
                if (_fader == null)
                {
                    var go = new GameObject("UIFader");
                    _fader = go.AddComponent<UIFader>();
                    DontDestroyOnLoad(go);
                }
            }

            UIManager?.Init();

            if (LifeManager)
            {
                if (!LifeManager.save) LifeManager.save = SaveManager;
                if (!LifeManager.ui) LifeManager.ui = UIManager;
                LifeManager.InitFromSave();
            }

            if (CrystalManager)
            {
                CrystalManager.ui = UIManager;
                UIManager?.UpdateCrystalUI("Crystal", CrystalManager.collectedTotal, CrystalManager.requiredTotal);
            }
            else UIManager?.UpdateCrystalUI("Crystal", 0, 0);

            SetState(GameState.MainMenu);
        }

        public void SetState(GameState s) => State = s;

        public bool IsPaused => _isPaused;
        public void PauseGame()
        {
            if (_isPaused) return;
            _isPaused = true;
            Time.timeScale = 0f;
            Physics2D.simulationMode = SimulationMode2D.Script;
            UIManager?.ShowPause();
            SetState(GameState.Paused);
        }
        public void ResumeGame()
        {
            if (!_isPaused) return;
            _isPaused = false;
            Time.timeScale = 1f;
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
            UIManager?.HidePause();
            SetState(GameState.Playing);
        }
        public void Pause() => PauseGame();
        public void Resume() => ResumeGame();

        private void OnDisable() { if (Time.timeScale == 0f) Time.timeScale = 1f; }
        private void OnDestroy() { if (Time.timeScale == 0f) Time.timeScale = 1f; }

        // -------- Level flow --------
        public void StartLevel(int levelId)
        {
            CurrentLevelId = levelId;

            Time.timeScale = 1f;
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

            if (LevelRoot == null)
                LevelRoot = new GameObject("LevelRoot").transform;

            EnsurePlayerSpawnedAndWired();

            UIManager?.BindLevelHUD();
            SetState(GameState.Playing);
        }

        public void RestartLevel()
        {
            if (LifeManager != null && LifeManager.Consume(1))
            {
                if (_player != null) _player.SetActive(false);
                StartLevel(CurrentLevelId);
            }
            else UIManager?.ShowNoLifeOptions();
        }

        public void OnLevelComplete()
        {
            Time.timeScale = 0f;
            Physics2D.simulationMode = SimulationMode2D.Script;
            SetState(GameState.LevelComplete);
            SaveManager?.MarkCompleted(CurrentLevelId);
            UIManager?.ShowLevelComplete();
        }

        public void OnPlayerDeath()
        {
            if (State == GameState.GameOver) return;
            SetState(GameState.GameOver);

            Time.timeScale = 0f;
            Physics2D.simulationMode = SimulationMode2D.Script;

            if (LifeManager != null && LifeManager.CanConsume(1))
            {
                LifeManager.Consume(1);
                UIManager?.ShowRetry();
            }
            else UIManager?.ShowNoLifeOptions();
        }

        public void LoadNextLevelWithFade(float fadeDuration = 0.5f)
        {
            StartCoroutine(CoLoadNextLevelWithFade(fadeDuration));
        }
        IEnumerator CoLoadNextLevelWithFade(float d)
        {
            if (_fader) yield return _fader.FadeTo(1f, d);
            CurrentLevelId += 1;
            yield return null;
            StartLevel(CurrentLevelId);
            yield return null;
            if (_fader) yield return _fader.FadeTo(0f, d);
        }

        // -------- Player spawn & wiring --------
        void EnsurePlayerSpawnedAndWired()
        {
            // 1) Bul / oluştur
            if (_player == null)
            {
                var existing = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
                if (existing != null) _player = existing;
                else if (PlayerPrefab != null)
                {
                    _player = Instantiate(PlayerPrefab);
                    _player.name = "Player";
                }
                else
                {
                    _player = new GameObject("Player");
                    _player.tag = "Player";
                    if (_player.GetComponent<BoxCollider2D>() == null) _player.AddComponent<BoxCollider2D>();
                    var rb = _player.GetComponent<Rigidbody2D>() ?? _player.AddComponent<Rigidbody2D>();
                    rb.gravityScale = 0; rb.freezeRotation = true;
                    TryAddComponent(_player, "LostPlanet.Gameplay.PlayerController");
                    TryAddComponent(_player, "LostPlanet.Gameplay.AbilityInventory");
                    TryAddComponent(_player, "LostPlanet.Gameplay.PlayerNOS");
                }
            }

            // 2) Parent
            if (LevelRoot != null && _player.transform.parent != LevelRoot)
                _player.transform.SetParent(LevelRoot, true);

            // 3) Spawn pozisyonu
            var spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
            var spawnPos = spawn ? spawn.transform.position : Vector3.zero;
            _player.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0);
            _player.transform.rotation = Quaternion.identity;

            // 4) Önce fizik GARANTİ: rb/col ekle, sonra görseli ayarla
            NormalizePlayerVisualAndPhysics(_player);

            // 5) Referansları bağla + grid’e oturt
            AutoWireCommonRefs(_player);

            _player.SetActive(true);
        }

        void NormalizePlayerVisualAndPhysics(GameObject go)
        {
            // --- Collider garanti ---
            if (!go.TryGetComponent<Collider2D>(out var col))
                col = go.AddComponent<BoxCollider2D>();

            // --- Rigidbody2D garanti (EKLENMEDEN ASLA ERİŞME) ---
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                try { rb = go.AddComponent<Rigidbody2D>(); }
                catch (System.Exception e)
                {
                    Debug.LogError($"[GM] {go.name} için Rigidbody2D eklenemedi: {e}");
                    return;
                }
            }

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // --- Sprite ölçek normalizasyonu (isteğe bağlı) ---
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float cell = (GridManager ? GridManager.cellSize : 1f);
                float targetH = cell * 0.9f;
                float h = sr.bounds.size.y;
                if (h > 0f)
                {
                    float scale = targetH / h;
                    go.transform.localScale = Vector3.one * scale;
                }
                sr.enabled = true;
                var c = sr.color; c.a = 1f; sr.color = c;
                if (sr.sortingOrder < 5) sr.sortingOrder = 5;
            }

            go.layer = LayerMask.NameToLayer("Default");
        }

        void AutoWireCommonRefs(GameObject player)
        {
            // InputManager (varsa)
            var input = FindByTypeName("LostPlanet.Input.InputManager");

            // Player üzerindeki tüm MonoBehaviour’larda yaygın alan/özellik isimlerini doldur
            foreach (var mb in player.GetComponents<MonoBehaviour>())
            {
                // Game ref
                TrySet(mb, "Game", this);
                TrySet(mb, "game", this);
                TrySet(mb, "GameManager", this);
                TrySet(mb, "gm", this);

                // Grid ref
                TrySet(mb, "GridManager", GridManager);
                TrySet(mb, "gridManager", GridManager);
                TrySet(mb, "Grid", GridManager);
                TrySet(mb, "grid", GridManager);

                // Input ref
                if (input != null)
                {
                    TrySet(mb, "Input", input);
                    TrySet(mb, "input", input);
                    TrySet(mb, "InputManager", input);
                    TrySet(mb, "inputManager", input);
                }
            }

            // GridPos ayarla + SnapToGrid çağır
            if (GridManager != null)
            {
                var gp = GridManager.WorldToGrid(player.transform.position);
                foreach (var mb in player.GetComponents<MonoBehaviour>())
                {
                    TrySet(mb, "GridPos", gp);
                    TrySet(mb, "gridPos", gp);
                }
                TryCallOn(player, "SnapToGrid");
            }

            // “Init” tarzı geri çağrılar
            TryCallOn(player, "OnSpawned");
            TryCallOn(player, "Init");

            // Varsayılan hız (bazı controller'larda 0 gelebiliyor)
            TrySetIfZero(player, "speed", 3f);
            TrySetIfZero(player, "moveSpeed", 3f);
        }

        // ---------- reflection helpers ----------
        static Component FindByTypeName(string qname)
        {
            var t = Type.GetType(qname);
            if (t == null) return null;
            var o = FindObjectOfType(t);
            return o as Component;
        }

        static void TryAddComponent(GameObject go, string qname)
        {
            var t = Type.GetType(qname);
            if (t != null && go.GetComponent(t) == null) go.AddComponent(t);
        }

        static void TrySet(object target, string fieldOrProp, object value)
        {
            if (target == null || value == null) return;
            var t = target.GetType();
            var f = t.GetField(fieldOrProp, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType.IsInstanceOfType(value)) { f.SetValue(target, value); return; }
            var p = t.GetProperty(fieldOrProp, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.CanWrite && p.PropertyType.IsInstanceOfType(value)) { p.SetValue(target, value, null); }
        }

        static void TryCallOn(GameObject go, string method)
        {
            foreach (var mb in go.GetComponents<MonoBehaviour>())
            {
                var mi = mb.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null && mi.GetParameters().Length == 0) mi.Invoke(mb, null);
            }
        }

        static void TrySetIfZero(GameObject go, string fieldOrProp, float valueIfZero)
        {
            foreach (var mb in go.GetComponents<MonoBehaviour>())
            {
                var t = mb.GetType();
                var f = t.GetField(fieldOrProp, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null && f.FieldType == typeof(float))
                {
                    float cur = (float)f.GetValue(mb);
                    if (Mathf.Approximately(cur, 0f)) f.SetValue(mb, valueIfZero);
                }
                var p = t.GetProperty(fieldOrProp, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.CanWrite && p.PropertyType == typeof(float))
                {
                    float cur = (float)(p.GetValue(mb, null) ?? 0f);
                    if (Mathf.Approximately(cur, 0f)) p.SetValue(mb, valueIfZero, null);
                }
            }
        }
    }
}
