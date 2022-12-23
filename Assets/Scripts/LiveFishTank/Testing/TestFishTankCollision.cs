using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFishTankCollision : MonoBehaviour
{
    [SerializeField] private Transform _target;

    void FixedUpdate()
    {
        transform.position = _target.position;
    }
}
