
using UnityEngine;

namespace LostPlanet.Enemies
{
    public class BossEnemy : EnemyBase
    {
        public int phase = 1;
        public override void TakeDashDamage(int dmg){ base.TakeDashDamage(dmg); if (hp == 3) phase = 2; }
        protected override void Tick()
        {
            if (phase == 1){ /* TODO */ }
            else { /* TODO */ }
        }
    }
}
