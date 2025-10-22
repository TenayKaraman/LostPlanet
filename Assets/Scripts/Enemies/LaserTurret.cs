
using UnityEngine;

namespace LostPlanet.Enemies
{
    public class LaserTurret : EnemyBase
    {
        public float rotateSpeed = 90f;
        protected override void Tick(){ transform.Rotate(0,0,rotateSpeed * stepInterval); }
    }
}
