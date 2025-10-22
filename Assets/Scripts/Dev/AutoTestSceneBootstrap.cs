using UnityEngine;
using LostPlanet.Core;
using LostPlanet.GridSystem;
using LostPlanet.Gameplay;
using LostPlanet.Items;
using LostPlanet.Interactives;
using LostPlanet.Enemies;
using LostPlanet.Dev; // RuntimeSprite helper'ı için

namespace LostPlanet.Dev
{
    // Boş sahneye ekleyip Play’e bas: portre (9x16) demo kurulumu
    public class AutoTestSceneBootstrap : MonoBehaviour
    {
        public GameObject playerPrefab; // boş ise runtime'da oluşturulur

        void Start()
        {
            FindObjectOfType<LostPlanet.Core.SaveManager>()?.DeleteKey("life");
            FindObjectOfType<LostPlanet.Core.SaveManager>()?.DeleteKey("nextRefillUtc");
            // Root
            var root = new GameObject("LevelRoot").transform;

            // Managers
            var gm = new GameObject("GameManager").AddComponent<GameManager>();
            var grid = new GameObject("GridManager").AddComponent<GridManager>();
            var save = new GameObject("SaveManager").AddComponent<SaveManager>();
            var audio = new GameObject("AudioManager").AddComponent<AudioManager>();
            var cm = new GameObject("CrystalManager").AddComponent<LostPlanet.Managers.CrystalManager>();
            var life = new GameObject("LifeManager").AddComponent<LostPlanet.Managers.LifeManager>();

            // Input
            var input = new GameObject("InputManager").AddComponent<LostPlanet.Input.InputManager>();

            // UIManager: varsa Canvas üzerindekini kullan, yoksa oluştur
            UIManager ui = FindObjectOfType<UIManager>();
            if (ui == null)
            {
                var uiGO = new GameObject("UIManager");
                ui = uiGO.AddComponent<UIManager>();
            }

            // Wire
            gm.GridManager = grid;
            gm.UIManager = ui;
            gm.SaveManager = save;
            gm.AudioManager = audio;
            gm.CrystalManager = cm;
            gm.LifeManager = life;
            gm.LevelRoot = root;

            // --------- GRID (PORTRE 9x16) + KAMERA ----------
            grid.width = 9;
            grid.height = 16;
            grid.cellSize = 1f;
            grid.RecalculateOrigin();

            var cam = Camera.main;
            if (cam)
            {
                cam.orthographicSize = grid.height * grid.cellSize * 0.5f; // 16 → 8
                cam.transform.position = new Vector3(0f, 0f, cam.transform.position.z);
            }

            // Küçük yardımcı: grid hücresine yerleştir ve occupant yaz
            Vector3 PlaceAtGrid(MonoBehaviour m, int gx, int gy)
            {
                gx = Mathf.Clamp(gx, 0, grid.width - 1);
                gy = Mathf.Clamp(gy, 0, grid.height - 1);
                var cell = new Vector2Int(gx, gy);
                var pos = grid.GridToWorld(cell);
                m.transform.position = pos;
                grid.SetOccupant(cell, m);
                return pos;
            }

            // --------- PLAYER PREFAB (template) ----------
            if (!playerPrefab) playerPrefab = BuildRuntimePlayerPrefab();
            gm.PlayerPrefab = playerPrefab;

            var templatesRoot = new GameObject("Templates").transform;
            templatesRoot.SetParent(root, false);
            playerPrefab.name = "PlayerTemplate";
            playerPrefab.transform.SetParent(templatesRoot, false);
            playerPrefab.SetActive(false);
            playerPrefab.hideFlags = HideFlags.HideInHierarchy;

            // Spawn marker (dünya 0,0 merkez)
            var sp = new GameObject("PlayerSpawn");
            sp.tag = "PlayerSpawn";
            sp.transform.position = Vector3.zero;

            // ----------------- PORTAL (sağ-üst) -----------------
            var portal = new GameObject("Portal").AddComponent<LostPlanet.Items.PortalController>();
            PlaceAtGrid(portal, grid.width - 2, grid.height - 4);  // (7,12)
            if (!portal.GetComponent<Collider2D>()) portal.gameObject.AddComponent<BoxCollider2D>();
            portal.GetComponent<Collider2D>().isTrigger = true;
            RuntimeSprite.EnsureRenderer(portal.gameObject, new Color(1f, 0f, 1f, 0.8f), 0, 1.0f);
            cm.portal = portal;
            cm.ui = ui;
            cm.requiredTotal = 3;

            // ----------------- KRİSTALLER (üst sıra) -----------------
            int[] xs = { 2, 4, 6 }; int yTop = grid.height - 3; // 13
            for (int i = 0; i < 3; i++)
            {
                var c = new GameObject("Crystal" + i).AddComponent<Crystal>();
                PlaceAtGrid(c, xs[i], yTop);
                var cCol = c.gameObject.AddComponent<CircleCollider2D>();
                cCol.isTrigger = true;
                RuntimeSprite.EnsureRenderer(c.gameObject, new Color(0.3f, 1f, 0.5f, 1f), 0, 0.8f);
            }

            // --- Ability Pool'u Resources'tan yükle (sürpriz kutular için)
            var defaultPool = Resources.Load<LostPlanet.ScriptableObjects.AbilityPoolDefinition>(
                "AbilityPools/DefaultAbilityPool");

            var shield = Resources.Load<LostPlanet.ScriptableObjects.AbilityDefinition>("Abilities/Shield");
            var emp = Resources.Load<LostPlanet.ScriptableObjects.AbilityDefinition>("Abilities/EMP");
            var bomb = Resources.Load<LostPlanet.ScriptableObjects.AbilityDefinition>("Abilities/Bomb");
            var picks = new[] { shield, emp, bomb };

            if (defaultPool == null)
            {
                Debug.LogWarning("[Bootstrap] AbilityPools/DefaultAbilityPool bulunamadı. " +
                                 "Create > LostPlanet > AbilityPool ile oluşturup Resources/AbilityPools içine taşıyın.");
            }

            // ----------------- POWER-UP KUTULARI (alt sıra) ----------
            int yBot = 2;
            for (int i = 0; i < 3; i++)
            {
                var box = new GameObject("PowerUpBox" + i).AddComponent<PowerUpBox>();
                box.pool = defaultPool;  // <-- HAVUZU BAĞLA

                // 1) Önce deterministik olarak bilinen kartları ata (debug/test)
                if (i < picks.Length && picks[i] != null)
                {
                    box.hasKnownCard = true;
                    box.knownCard = picks[i];
                }
                // 2) Eğer bilinen kart bulunamazsa, pool’dan sürpriz çek (release davranışı)
                else if (defaultPool != null)
                {
                    box.pool = defaultPool;
                }

                PlaceAtGrid(box, xs[i], yBot);
                var bCol = box.gameObject.AddComponent<BoxCollider2D>(); bCol.isTrigger = true;
                RuntimeSprite.EnsureRenderer(box.gameObject, new Color(1f, 0.85f, 0.2f, 1f), 0, 0.9f);
            }

            // ----------------- TUZAKLAR -----------------
            var spike = new GameObject("Spike").AddComponent<SpikeTrap>();
            PlaceAtGrid(spike, grid.width / 2, 3); // (4,3)
            var spCol = spike.gameObject.AddComponent<BoxCollider2D>();
            spCol.isTrigger = true;
            RuntimeSprite.EnsureRenderer(spike.gameObject, new Color(1f, 0.25f, 0.25f, 1f), 0, 1f);

            var laser = new GameObject("LaserTrap").AddComponent<TimedLaserTrap>();
            PlaceAtGrid(laser, grid.width / 2 + 2, grid.height / 2); // (6,8)
            if (!laser.beamCollider) laser.beamCollider = laser.gameObject.AddComponent<BoxCollider2D>();
            laser.beamCollider.isTrigger = true;
            if (!laser.beamVisual)
            {
                var beam = new GameObject("Beam");
                beam.transform.SetParent(laser.transform, false);
                RuntimeSprite.EnsureRenderer(beam, new Color(0.2f, 1f, 1f, 0.8f), 1, 1.2f);
                laser.beamVisual = beam;
            }
            RuntimeSprite.EnsureRenderer(laser.gameObject, new Color(0.2f, 0.7f, 1f, 0.6f), 0, 0.6f);

            // ----------------- DÜŞMANLAR -----------------
            var chaser = new GameObject("Chaser").AddComponent<ChaserEnemy>();
            chaser.Grid = grid; chaser.GridPos = new Vector2Int(grid.width - 3, grid.height - 6); // (6,10)
            chaser.SnapToGrid();
            RuntimeSprite.EnsureRenderer(chaser.gameObject, new Color(1f, 0.5f, 0.2f, 1f), 0, 1f);

            var patrol = new GameObject("Patrol").AddComponent<PatrolEnemy>();
            patrol.Grid = grid; patrol.GridPos = new Vector2Int(1, grid.height / 2 - 2); // (1,6)
            patrol.SnapToGrid();
            patrol.waypoints = new Vector2Int[] { new Vector2Int(1, 6), new Vector2Int(3, 6) };
            RuntimeSprite.EnsureRenderer(patrol.gameObject, new Color(0.4f, 0.7f, 1f, 1f), 0, 1f);

            // GO!
            gm.Init();
            gm.StartLevel(1);
        }

        GameObject BuildRuntimePlayerPrefab()
        {
            var go = new GameObject("Player");
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Rigidbody2D>().gravityScale = 0;
            go.AddComponent<PlayerController>();
            go.AddComponent<AbilityInventory>();
            go.AddComponent<PlayerNOS>();
            RuntimeSprite.EnsureRenderer(go, Color.white, 5, 0.9f);
            return go;
        }
    }
}
