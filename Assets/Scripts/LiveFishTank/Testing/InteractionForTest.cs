using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionForTest : MonoBehaviour
{
    [SerializeField] private Transform _spawnLocation;
    [SerializeField] private GameObject _fish;
    [SerializeField] private GameObject _food;
    [SerializeField] private FishTank _tank;

    void Start()
    {
        _tank.ToggleVisibility(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DropFish();
        }
        if (Input.GetMouseButtonDown(1))
        {
            DropFood();
        }
    }

    private void DropFood()
    {
        var fishFoodGroup = Instantiate(
            _food,
            _spawnLocation.position,
            Quaternion.identity)
            .GetComponent<TankResident>();
        fishFoodGroup.Init(0, _tank);
    }

    private void DropFish()
    {
        Debug.Log("fish drop");
        var angle = Random.Range(30f, 180f);
        var fish = Instantiate(
            _fish,
            _spawnLocation.position,
            Quaternion.AngleAxis(angle, Vector3.up))
            .GetComponent<TankResident>();
        fish.Init(0, _tank);
        Debug.Log("fish done init");
    }
}
