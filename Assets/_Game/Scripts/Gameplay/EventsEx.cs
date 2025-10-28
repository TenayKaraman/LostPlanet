using System;
using UnityEngine;
using UnityEngine.Events;
using UInput = UnityEngine.Input;

namespace LostPlanet.Gameplay
{
    [Serializable] public class StringEvent : UnityEvent<string> { }
    [Serializable] public class IntEvent : UnityEvent<int> { }
    [Serializable] public class VoidEvent : UnityEvent { }
}
