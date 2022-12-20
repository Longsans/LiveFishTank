using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishEventHandlers : MonoBehaviour
{
    private Fish _fish;

    void Start()
    {
        _fish = GetComponentInParent<Fish>();
    }

    void OnTriggerStay(Collider other)
    {
        _fish.HandleFishTriggerStay(other);
    }
}
