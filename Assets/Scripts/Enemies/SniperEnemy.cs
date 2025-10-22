
using UnityEngine;

namespace LostPlanet.Enemies
{
    public class SniperEnemy : EnemyBase
    {
        public float fireCooldown = 1.0f;
        float _cool;

        protected override void Tick()
        {
            var player = FindObjectOfType<LostPlanet.Gameplay.PlayerController>();
            if (player == null) return;
            if (SameRowOrCol(player))
            {
                _cool -= stepInterval;
                if (_cool <= 0f) { Shoot(player); _cool = fireCooldown; }
            }
        }

        bool SameRowOrCol(LostPlanet.Gameplay.PlayerController p) => p.GridPos.x == GridPos.x || p.GridPos.y == GridPos.y;
        void Shoot(LostPlanet.Gameplay.PlayerController p){ LostPlanet.Core.GameManager.Instance.OnPlayerDeath(); }
    }
}
