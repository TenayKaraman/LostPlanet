
using UnityEngine;

namespace LostPlanet.Interactives
{
    public class SensorPlate : MonoBehaviour
    {
        public string doorId;
        void OnTriggerEnter2D(Collider2D other){ LostPlanet.Core.Events.RaiseSensor(doorId, true); }
        void OnTriggerExit2D(Collider2D other){ LostPlanet.Core.Events.RaiseSensor(doorId, false); }
    }

    public class DoorController : MonoBehaviour
    {
        public string doorId;
        public bool IsOpen { get; private set; } = false;
        public Collider2D solidCollider;

        void OnEnable(){ LostPlanet.Core.Events.OnSensor += HandleSensor; }
        void OnDisable(){ LostPlanet.Core.Events.OnSensor -= HandleSensor; }

        void HandleSensor(string id, bool state)
        {
            if (id != doorId) return;
            IsOpen = state;
            if (solidCollider) solidCollider.enabled = !IsOpen;
        }
    }
}
