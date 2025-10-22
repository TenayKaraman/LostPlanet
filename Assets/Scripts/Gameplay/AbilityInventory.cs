using UnityEngine;
using System.Collections;
using LostPlanet.ScriptableObjects;
using LostPlanet.Core;
using DG.Tweening; // DOTween punch/flash için

namespace LostPlanet.Gameplay
{
    public class AbilityInventory : MonoBehaviour
    {
        public const int SlotCount = 3;

        // Slotlar boþluk = null
        public AbilityDefinition[] slots = new AbilityDefinition[SlotCount];

        // Shield
        private bool shieldActive = false;
        private float shieldUntil = 0f;

        // Phase
        private bool phaseActive = false;
        private float phaseUntil = 0f;

        void OnEnable()
        {
            // UI'ý baþlangýçta temizle/senkronla
            Notify();
        }

        // ---- Kart Ekleme ----
        public bool AddCard(AbilityDefinition def)
        {
            if (def == null) return false;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) // boþluk kontrolü
                {
                    slots[i] = def; // doðrudan karta referans

                    var ui = GameManager.Instance ? GameManager.Instance.UIManager : FindObjectOfType<UIManager>();
                    ui?.SetAbilitySlot(i, def.abilityId);

                    // SFX — kart alým sesi (ulaþýlabilir yere taþýndý)
                    var am = GameManager.Instance ? GameManager.Instance.AudioManager : null;
                    am?.PlaySfx(am?.pickupSfx);

                    Debug.Log($"[AbilityInventory] Slot {i} <= {def.abilityId}");
                    return true;
                }
            }

            Debug.Log("[AbilityInventory] No free slot");
            return false;
        }

        // ---- Slot Kullanma ----
        public void UseSlot(int index)
        {
            if (index < 0 || index >= SlotCount) return;
            var def = slots[index];
            if (def == null) return;

            // Görsel "punch" ve kýsa flaþ
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 8, 0.8f);

            var sr = GetComponent<SpriteRenderer>();
            if (sr) sr.DOColor(Color.white, 0.08f).From(new Color(1f, 1f, 1f, 0.4f));

            StartCoroutine(ActivateRoutine(index, def));

            // Slotu hemen boþalt ve UI'ý güncelle (kart tek kullanýmlýk)
            slots[index] = null;
            Notify();
        }

        IEnumerator ActivateRoutine(int slotIndex, AbilityDefinition def)
        {
            float dur = Mathf.Max(0.01f, def.duration);
            var am = GameManager.Instance ? GameManager.Instance.AudioManager : null;

            switch (def.abilityId)
            {
                case "Shield":
                    am?.PlaySfx(am?.shieldSfx);
                    shieldActive = true; shieldUntil = Time.time + dur;
                    yield return StartCoroutine(RunDuration(slotIndex, dur, null, () => shieldActive = false));
                    yield break;

                case "EMP":
                    am?.PlaySfx(am?.empSfx);
                    yield return StartCoroutine(RunDuration(slotIndex, dur, () => EMP_SetDisabled(true), () => EMP_SetDisabled(false)));
                    yield break;

                case "Phase":
                    am?.PlaySfx(am?.phaseSfx);
                    phaseActive = true; phaseUntil = Time.time + dur;
                    var col = GetComponent<Collider2D>();
                    bool oldTrigger = col ? col.isTrigger : false;
                    if (col) col.isTrigger = true;
                    yield return StartCoroutine(RunDuration(slotIndex, dur, null, () =>
                    {
                        phaseActive = false;
                        if (col) col.isTrigger = oldTrigger;
                    }));
                    yield break;

                case "Bomb":
                    am?.PlaySfx(am?.bombSfx);
                    DoBomb((int)Mathf.Round(def.power));
                    GameManager.Instance?.UIManager?.SetSlotCooldown01(slotIndex, 0f);
                    yield break;

                case "Freeze":
                    am?.PlaySfx(am?.freezeSfx);
                    yield return StartCoroutine(RunDuration(slotIndex, dur, () => Freeze_All(true), () => Freeze_All(false)));
                    yield break;

                default:
                    yield break;
            }
        }

        IEnumerator RunDuration(int slotIndex, float duration, System.Action onStart, System.Action onEnd)
        {
            onStart?.Invoke();

            float t = duration;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                GameManager.Instance?.UIManager?.SetSlotCooldown01(slotIndex, Mathf.Clamp01(t / duration));
                yield return null;
            }

            GameManager.Instance?.UIManager?.SetSlotCooldown01(slotIndex, 0f);
            onEnd?.Invoke();
        }

        // --- Queries ---
        public bool HasActiveShield() => shieldActive && Time.time < shieldUntil;
        public void ConsumeShield() { shieldActive = false; }
        public bool IsPhaseActive() => phaseActive && Time.time < phaseUntil;

        // --- EMP: disable traps
        void EMP_SetDisabled(bool on)
        {
            var traps = GameObject.FindObjectsOfType<LostPlanet.Interactives.Trap>();
            foreach (var t in traps) t.SetDisabled(on);
        }

        // --- Freeze: freeze enemies & traps
        void Freeze_All(bool on)
        {
            var enemies = GameObject.FindObjectsOfType<LostPlanet.Enemies.EnemyBase>();
            foreach (var e in enemies) e.SetFrozen(on);

            var traps = GameObject.FindObjectsOfType<LostPlanet.Interactives.Trap>();
            foreach (var t in traps) t.SetFrozen(on);
        }

        // --- Bomb: AoE clear (Manhattan radius = power)
        void DoBomb(int radius)
        {
            var grid = FindObjectOfType<LostPlanet.GridSystem.GridManager>();
            var pc = GetComponent<PlayerController>();
            if (grid == null || pc == null) return;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) > radius) continue;

                    var c = pc.GridPos + new Vector2Int(dx, dy);
                    if (!grid.IsInside(c)) continue;

                    var occ = grid.cells[c.x, c.y].occupant;
                    if (occ is LostPlanet.Enemies.EnemyBase e) e.Die();
                }
            }
        }

        // ---- UI Senkronizasyonu ----
        void Notify()
        {
            var ui = GameManager.Instance ? GameManager.Instance.UIManager : FindObjectOfType<UIManager>();
            if (ui == null) return;

            for (int i = 0; i < SlotCount; i++)
            {
                if (slots[i] != null)
                {
                    ui.SetAbilitySlot(i, slots[i].abilityId);
                }
                else
                {
                    ui.ClearAbilitySlot(i);
                }
                ui.SetSlotCooldown01(i, 0f);
            }
        }
    }
}
