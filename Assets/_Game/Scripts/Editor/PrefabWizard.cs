#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using LostPlanet.Gameplay;

namespace LostPlanet.EditorTools
{
    public static class PrefabWizard
    {
        const string Root = "Assets/_Game";
        const string PfRoot = Root + "/Prefabs";
        static string PathPlayer => PfRoot + "/Player";
        static string PathCollectibles => PfRoot + "/Collectibles";
        static string PathPortals => PfRoot + "/Portals";
        static string PathEnemies => PfRoot + "/Enemies";
        static string PathInteractables => PfRoot + "/Interactables";
        static string PathMisc => PfRoot + "/Misc";

        [MenuItem("Tools/LostPlanet/Setup/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
            CreateIfMissing(Root);
            CreateIfMissing(PfRoot);
            CreateIfMissing(PathPlayer);
            CreateIfMissing(PathCollectibles);
            CreateIfMissing(PathPortals);
            CreateIfMissing(PathEnemies);
            CreateIfMissing(PathInteractables);
            CreateIfMissing(PathMisc);
            AssetDatabase.Refresh();
            Debug.Log("[PrefabWizard] Default folders created.");
        }

        [MenuItem("Tools/LostPlanet/Prefabs/Prefabize Selection (by Tag)")]
        public static void PrefabizeSelection()
        {
            CreateDefaultFolders();

            foreach (var obj in Selection.gameObjects)
            {
                var go = obj;

                // Otomatik bileþen ekleme
                AutoAttachByGuess(go);

                string dir = GuessFolderByTag(go.tag);
                string file = $"{dir}/{Sanitize(go.name)}.prefab";
                file = AssetDatabase.GenerateUniqueAssetPath(file);

                var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, file, InteractionMode.AutomatedAction);
                if (prefab != null)
                    Debug.Log($"[PrefabWizard] Saved: {file}");
            }
        }

        static string GuessFolderByTag(string tag)
        {
            switch (tag)
            {
                case "Player": return PathPlayer;
                case "Collectible": return PathCollectibles;
                case "Portal": return PathPortals;
                case "Enemy": return PathEnemies;
                case "Interactable": return PathInteractables;
                default: return PathMisc;
            }
        }

        static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }

        static void CreateIfMissing(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = Path.GetDirectoryName(path).Replace("\\", "/");
                var leaf = Path.GetFileName(path);
                if (string.IsNullOrEmpty(parent) || parent == "Assets")
                    AssetDatabase.CreateFolder("Assets", leaf);
                else
                    AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        static void AutoAttachByGuess(GameObject go)
        {
            var t = go.tag;

            if (t == "Collectible" && go.GetComponent<CrystalCollectible>() == null)
            {
                go.AddComponent<CrystalCollectible>();
                if (go.GetComponent<BobAndSpin>() == null) go.AddComponent<BobAndSpin>();
            }
            else if (t == "Interactable" && go.name.ToLower().Contains("ability") &&
                     go.GetComponent<AbilityBox>() == null)
            {
                go.AddComponent<AbilityBox>();
            }
            else if (t == "Portal" && go.GetComponent<ExitPortal>() == null)
            {
                go.AddComponent<ExitPortal>();
            }
            else if (t == "Enemy" && go.GetComponent<EnemyPatrol>() == null)
            {
                go.AddComponent<EnemyPatrol>();
            }
        }
    }
}
#endif
