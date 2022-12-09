using UnityEngine;

public class Fish : LocalObject
{
    // fish grows when max growth points at each level is reached
    // each level has more max growth points than the last
    [SerializeField] private int _baseGrowthPoints = 120;

    // growth speed in growth point per minute
    [SerializeField] private int _growthSpeed = 1;

    // swim speed in m/s
    [SerializeField] private float _swimSpeed = 0.5f;

    private int _growthLevel = 1;
    private int _levelGrowthPoints;
    private int _currentGrowthPoints = 0;

    // a multiplier to apply to scale, increases with growth level
    private float _size = 1f;
    private FishFood _foodHeadedFor;
    private bool _headingTowardsFood = false;
    private FishTank _tank;

    void FixedUpdate()
    {
        if (_headingTowardsFood)
        {
            Vector3 direction = _foodHeadedFor.transform.position - transform.position;
            transform.Translate(direction * Time.fixedDeltaTime, Space.World);
            return;
        }
        if (_foodHeadedFor && !_headingTowardsFood)
        {
            // because the forward-facing direction of the fish model is not its Z axis,
            // we have to correct it after applying LookRotation
            var angleToForwardDirection = 90f;
            transform.rotation =
                Quaternion.LookRotation(
                    _foodHeadedFor.transform.position - transform.position) * Quaternion.AngleAxis(angleToForwardDirection, Vector3.up);
            _headingTowardsFood = true;
            return;
        }

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

    void Update()
    {
        if (!_foodHeadedFor)
        {
            var foodGroup = _tank.FindFishFoodGroupInTank();
            if (foodGroup)
                _foodHeadedFor = foodGroup.GetRandomFoodPiece();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // consume the first food this fish comes into contact with
        if (other.TryGetComponent<FishFood>(out var fishFood))
        {
            StatusLog.Instance.DebugLog("Fish collided with fish food");
            _foodHeadedFor = fishFood;
            _currentGrowthPoints += _foodHeadedFor.GrowthPoints;
            while (_currentGrowthPoints >= _levelGrowthPoints)
            {
                _currentGrowthPoints -= _levelGrowthPoints;
                _levelGrowthPoints = ++_growthLevel * _baseGrowthPoints;
                _size += 0.5f;
            }
            var consumedFood = _foodHeadedFor;
            ResetToNormalState();
            consumedFood.OnConsumed();
        }
    }

    public override void Init(int prefabIndex, GeospatialObject geoObject)
    {
        base.Init(prefabIndex, geoObject);
        SetUpWithTank();
        _levelGrowthPoints = _baseGrowthPoints;
        _saveData.Type = LocalObjectType.Fish;
        var fishTank = geoObject.GetComponent<FishTank>();
    }

    public override void Save()
    {
        base.Save();
        var fishData = new FishData
        {
            BaseGrowthPoints = _baseGrowthPoints,
            LevelGrowthPoints = _levelGrowthPoints,
            CurrentGrowthPoints = _currentGrowthPoints,
            GrowthLevel = _growthLevel,
            GrowthSpeed = _growthSpeed,
            SwimSpeed = _swimSpeed,
            Size = _size
        };
        _saveData.OtherData = JsonUtility.ToJson(fishData);
    }

    public override void Restore(LocalObjectData localData, GeospatialObject geoObject)
    {
        base.Restore(localData, geoObject);
        SetUpWithTank();
        var fishData = JsonUtility.FromJson<FishData>(localData.OtherData);
        _baseGrowthPoints = fishData.BaseGrowthPoints;
        _levelGrowthPoints = fishData.LevelGrowthPoints;
        _currentGrowthPoints = fishData.CurrentGrowthPoints;
        _growthLevel = fishData.GrowthLevel;
        _growthSpeed = fishData.GrowthSpeed;
        _swimSpeed = fishData.SwimSpeed;
        _size = fishData.Size;
        transform.localScale *= _size;
    }

    private void ResetToNormalState()
    {
        _headingTowardsFood = false;
        _foodHeadedFor = null;
        transform.rotation *= Quaternion.AngleAxis(-transform.rotation.eulerAngles.z, Vector3.forward);
    }

    private void HandleFoodPieceConsumed(FishFood consumedFood)
    {
        if (consumedFood == _foodHeadedFor)
            ResetToNormalState();
    }

    private void SetUpWithTank()
    {
        _tank = _geoObject.GetComponent<FishTank>();
        _tank.FoodPieceConsumed.AddListener(HandleFoodPieceConsumed);
    }
}

public class FishData
{
    public int BaseGrowthPoints;
    public int LevelGrowthPoints;
    public int CurrentGrowthPoints;
    public int GrowthLevel;
    public int GrowthSpeed;
    public float SwimSpeed;
    public float Size;
}