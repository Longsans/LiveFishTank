using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFood : MonoBehaviour
{
    public int GrowthPoints => _growthPoints;
    [SerializeField] private int _growthPoints;
    private FishFoodGroup _foodGroup;

    void Awake()
    {
        _foodGroup = GetComponentInParent<FishFoodGroup>();
    }

    public void OnConsumed()
    {
        _foodGroup.OnFoodPieceConsumed(this);
    }
}
