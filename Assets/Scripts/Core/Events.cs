
using System;

namespace LostPlanet.Core
{
    public static class Events
    {
        public static event Action<string,bool> OnSensor;
        public static void RaiseSensor(string id, bool state) => OnSensor?.Invoke(id, state);
    }
}
