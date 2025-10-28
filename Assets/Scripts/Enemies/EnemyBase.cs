
using UnityEngine;
using LostPlanet.GridSystem;
using System.Collections.Generic;

namespace LostPlanet.Enemies
{
    public enum EnemyState { Idle, Patrol, Chase, Aim, Shoot, Dead }
    public static readonly List<EnemyBase> All = new List<EnemyBase>();
    public class EnemyBase : GridEntity
    {
        public EnemyState State = EnemyState.Idle;
        public bool isBoss = false;
        public int hp = 1;
        public float stepInterval = 0.25f;
        float _timer;
        bool frozen = false;

        protected virtual void Update()
        {
            if (State == EnemyState.Dead || frozen) return;
            _timer += Time.deltaTime;
            if (_timer >= stepInterval) { _timer = 0f; Tick(); }
        }

        protected virtual void Tick() { }

        public virtual void Die(){ State = EnemyState.Dead; Destroy(gameObject); }
        public virtual void TakeDashDamage(int dmg){ hp -= dmg; if (hp <= 0) Die(); }

        public void SetFrozen(bool on){ frozen = on; }
    }
        protected virtual void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        protected virtual void OnDisable() { All.Remove(this); }
}
