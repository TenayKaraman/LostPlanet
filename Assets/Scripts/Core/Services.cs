
using UnityEngine;

namespace LostPlanet.Core
{
    /// <summary>
    /// Çok hafif bir Service Locator.
    /// İlk erişimde sahneden bulur ve cache'ler; isteğe bağlı olarak Register* ile elle set edilebilir.
    /// </summary>
    public static class Services
    {
        static UIManager _ui;
        static SaveManager _save;
        static LifeManager _life;
        static Managers.CrystalManager _crystal;
        static AudioManager _audio;
        static GridSystem.GridManager _grid;
        static GameManager _game;

        public static UIManager UI => _ui ? _ui : (_ui = Object.FindObjectOfType<UIManager>());
        public static SaveManager Save => _save ? _save : (_save = Object.FindObjectOfType<SaveManager>());
        public static LifeManager Life => _life ? _life : (_life = Object.FindObjectOfType<LifeManager>());
        public static Managers.CrystalManager Crystal => _crystal ? _crystal : (_crystal = Object.FindObjectOfType<Managers.CrystalManager>());
        public static AudioManager Audio => _audio ? _audio : (_audio = Object.FindObjectOfType<AudioManager>());
        public static GridSystem.GridManager Grid => _grid ? _grid : (_grid = Object.FindObjectOfType<GridSystem.GridManager>());
        public static GameManager Game => _game ? _game : (_game = Object.FindObjectOfType<GameManager>());

        public static void Register(UIManager ui) => _ui = ui;
        public static void Register(SaveManager s) => _save = s;
        public static void Register(LifeManager l) => _life = l;
        public static void Register(Managers.CrystalManager c) => _crystal = c;
        public static void Register(AudioManager a) => _audio = a;
        public static void Register(GridSystem.GridManager g) => _grid = g;
        public static void Register(GameManager g) => _game = g;
    }
}
