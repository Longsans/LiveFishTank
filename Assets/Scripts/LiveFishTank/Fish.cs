using UnityEngine;

public class Fish : LocalObject
{
    // growth period in minutes
    [SerializeField] private int _growthPeriod;
    [SerializeField] private float _swimSpeed = 0.5f;

    // a multiplier to apply to scale, increases with fish growth
    private float _size = 1f;

    public override void Init(int prefabIndex, GeospatialObject geoObject)
    {
        base.Init(prefabIndex, geoObject);
        _saveData.Type = LocalObjectType.Fish;
    }

    public override void Save()
    {
        base.Save();
        var fishData = new FishData
        {
            GrowthPeriod = _growthPeriod,
            SwimSpeed = _swimSpeed,
            Size = _size
        };
        _saveData.OtherData = JsonUtility.ToJson(fishData);
    }

    public override void Restore(LocalObjectData localData, GeospatialObject geoObject)
    {
        base.Restore(localData, geoObject);
        var fishData = JsonUtility.FromJson<FishData>(localData.OtherData);
        _growthPeriod = fishData.GrowthPeriod;
        _swimSpeed = fishData.SwimSpeed;
        _size = fishData.Size;
        transform.localScale *= _size;
    }

    void FixedUpdate()
    {
        // fish prefab model's actual forward-facing direction is its -X axis
        var destination = transform.position - Time.fixedDeltaTime * _swimSpeed * transform.right;
        if (!_geoObject.Collider.bounds.Contains(destination))
        {
            var angle = Random.Range(30f, 180f);
            transform.Rotate(Vector3.up, angle);
            return;
        }
        transform.position = destination;
    }
}

public class FishData
{
    public int GrowthPeriod;
    public float SwimSpeed;
    public float Size;
}