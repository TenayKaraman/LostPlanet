using UnityEngine;
using UnityEngine.UI;
using LostPlanet.Gameplay;

public class AbilityBarController : MonoBehaviour
{
    public Button slot0, slot1, slot2;
    AbilityInventory inv;

    void Start()
    {
        inv = FindObjectOfType<AbilityInventory>();
        if (slot0) slot0.onClick.AddListener(() => Use(0));
        if (slot1) slot1.onClick.AddListener(() => Use(1));
        if (slot2) slot2.onClick.AddListener(() => Use(2));
    }

    void Use(int i)
    {
        if (inv == null) inv = FindObjectOfType<AbilityInventory>();
        inv?.UseSlot(i);
    }
}
