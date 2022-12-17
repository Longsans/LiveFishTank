using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FishTank : GeospatialObject
{
    [HideInInspector] public string Name;
    [HideInInspector] public UnityEvent<FishFood> FoodPieceConsumed;
    private DimBoxes.BoundBox _visualization;
    private int _nextFoodGroup = 0;

    protected override void Awake()
    {
        base.Awake();
        _visualization = GetComponentInChildren<DimBoxes.BoundBox>();
        PlaceablesManager.Instance.ShowGeospatialObjectsBoundsChanged
            .AddListener(ToggleBounds);
    }

    public override void Init()
    {
        base.Init();
        Name = $"Fish tank {PlaceablesManager.Instance.FishTanksCount + 1}";
    }

    public override void Save()
    {
        base.Save();
        SaveData.OtherData = JsonUtility.ToJson(
            new FishTankData
            {
                Name = Name,
                TankSize = Collider.gameObject.transform.localScale,
            });
    }

    public override void Restore(GeospatialObjectData geoData, Transform anchor)
    {
        base.Restore(geoData, anchor);
        var tankData = JsonUtility.FromJson<FishTankData>(geoData.OtherData);
        Name = tankData.Name;
        Collider.gameObject.transform.localScale = tankData.TankSize;
    }

    public override void ToggleBounds(bool show)
    {
        if (InteractionManager.Instance.CurrentSelectedPlaceable != gameObject)
            _visualization.enabled = show;
    }

    public FishFoodGroup FindFishFoodGroupInTank()
    {
        foreach (var localObject in _localObjects)
        {
            if (localObject.TryGetComponent<FishFoodGroup>(out var foodGroup))
            {
                return foodGroup;
            }
        }
        return null;
    }

    public FishFood GetNextFishFoodInTank()
    {
        var foodGroups =
            _localObjects.Where(obj => obj.TryGetComponent<FishFoodGroup>(out var foodGroup))
                        .Select(obj => obj.GetComponent<FishFoodGroup>())
                        .ToList();
        if (foodGroups.Count == 0)
            return null;

        FishFood food;
        if (_nextFoodGroup >= foodGroups.Count)
            _nextFoodGroup = 0;

        while (!(food = foodGroups[_nextFoodGroup].GetNextFoodPiece()))
        {
            _nextFoodGroup = ++_nextFoodGroup % foodGroups.Count;
            if (_nextFoodGroup == 0)
            {
                foreach (var foodGroup in foodGroups)
                    foodGroup.ResetFoodPiecesIterator();
            }
        }
        return food;
    }

    public void NotifyFoodPieceConsumed(FishFood consumedPiece)
    {
        FoodPieceConsumed.Invoke(consumedPiece);
    }

    public void SetTankWidth(float width)
    {
        Collider.gameObject.transform.localScale = new Vector3(
            width,
            Collider.gameObject.transform.localScale.y,
            Collider.gameObject.transform.localScale.z);
    }

    public void SetTankLength(float length)
    {
        Collider.gameObject.transform.localScale = new Vector3(
            Collider.gameObject.transform.localScale.x,
            Collider.gameObject.transform.localScale.y,
            length);
    }

    public void SetTankHeight(float height)
    {
        Collider.gameObject.transform.localScale = new Vector3(
            Collider.gameObject.transform.localScale.x,
            height,
            Collider.gameObject.transform.localScale.z);
    }
}

public class FishTankData
{
    public string Name;
    public Vector3 TankSize;
}