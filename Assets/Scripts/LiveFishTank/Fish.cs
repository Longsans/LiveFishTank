using UnityEngine;
using UnityEngine.Events;
using System;

public class Fish : LocalObject
{
    [HideInInspector] public float MaxSatiety => _levelMaxSatiety;
    [HideInInspector] public float CurrentSatiety => _currentSatiety;
    [HideInInspector] public bool IsFull => _currentSatiety == _levelMaxSatiety;
    [HideInInspector] public UnityEvent AllDataInitialized;

    // fish grows when max growth points at each level is reached
    // each level has more max growth points than the last
    [SerializeField] private int _baseGrowthPoints = 120;

    // fish can consume maximum 5 food pieces before it's "full"
    [SerializeField] private float _baseMaxSatiety = 5f;

    [SerializeField] private float _baseSatietyDropPerHour = 1f;
    [SerializeField] private float _satietyIncreasePerFood = 1f;
    [SerializeField] private float _satietyIncreasePerLevel = 2.5f;
    [SerializeField] private float _satietyDropIncreasePerLevel = 0.25f;

    // growth speed in growth points per minute, when satiety > 0
    [SerializeField] private float _growthPerHourWhenFed = 30f;
    [SerializeField] private float _sizeIncreasePerLevel = 0.2f;

    // swim speed in m/s
    [SerializeField] private float _swimSpeed = 0.5f;

    private int _growthLevel = 1;
    private int _levelGrowthPoints;
    private int _currentGrowthPoints = 0;

    // same as growth points, max Satiety increases with level
    private float _levelMaxSatiety;
    private float _levelSatietyDropPerHour;
    private float _currentSatiety = 0f;

    // a multiplier to apply to scale, increases with growth level
    private float _size = 1f;
    private DateTime _lastSatietyChange;
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
            var angle = UnityEngine.Random.Range(30f, 180f);
            transform.Rotate(Vector3.up, angle);
            return;
        }
        transform.position = destination;
    }

    void Update()
    {
        if (!_foodHeadedFor && !IsFull)
        {
            var foodGroup = _tank.FindFishFoodGroupInTank();
            if (foodGroup)
                _foodHeadedFor = foodGroup.GetRandomFoodPiece();
        }
        GrowAccordingToRemainingSatiety();
    }

    void OnTriggerEnter(Collider other)
    {
        // consume the first food this fish comes into contact with
        if (!IsFull && other.TryGetComponent<FishFood>(out var fishFood))
        {
            ConsumeFood(fishFood);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // if fish previously was full on first contact but becomes less full afterwards
        if (!IsFull && other.TryGetComponent<FishFood>(out var fishFood))
        {
            ConsumeFood(fishFood);
        }
    }

    public override void Init(int prefabIndex, GeospatialObject geoObject)
    {
        base.Init(prefabIndex, geoObject);
        SetUpWithTank();
        CalculateCurrentLevelAttributes();
        _lastSatietyChange = DateTime.Now;
        _saveData.Type = LocalObjectType.Fish;
        var fishTank = geoObject.GetComponent<FishTank>();
        AllDataInitialized.Invoke();
    }

    public override void Save()
    {
        base.Save();
        var fishData = new FishData
        {
            GrowthLevel = _growthLevel,
            CurrentGrowthPoints = _currentGrowthPoints,
            CurrentSatiety = _currentSatiety,
            LastSatietyChange = _lastSatietyChange,
            Size = _size
        };
        _saveData.OtherData = JsonUtility.ToJson(fishData);
    }

    public override void Restore(LocalObjectData localData, GeospatialObject geoObject)
    {
        base.Restore(localData, geoObject);
        SetUpWithTank();
        var fishData = JsonUtility.FromJson<FishData>(localData.OtherData);
        _growthLevel = fishData.GrowthLevel;
        _currentGrowthPoints = fishData.CurrentGrowthPoints;
        _currentSatiety = fishData.CurrentSatiety;
        _lastSatietyChange = fishData.LastSatietyChange;
        _size = fishData.Size;

        CalculateCurrentLevelAttributes();
        GrowAccordingToRemainingSatiety();
        transform.localScale *= _size;
        AllDataInitialized.Invoke();
    }

    private void ConsumeFood(FishFood food)
    {
        _foodHeadedFor = food;
        _currentGrowthPoints += _foodHeadedFor.GrowthPoints;
        _currentSatiety = Math.Clamp(_currentSatiety + _satietyIncreasePerFood, 0f, _levelMaxSatiety);
        _lastSatietyChange = DateTime.Now;
        GrowUsingGrowthPoints();

        var consumedFood = _foodHeadedFor;
        ResetToNormalState();
        consumedFood.OnConsumed();
    }

    /// <summary>
    /// Consume remaining satiety to grow over time, gaining growth points each couple minutes
    /// </summary>
    private void GrowAccordingToRemainingSatiety()
    {
        var timeSinceLastSatietyChange = DateTime.Now - _lastSatietyChange;
        var droppedSatiety = (float)Math.Clamp(
            _levelSatietyDropPerHour * timeSinceLastSatietyChange.TotalHours,
            0f,
            _currentSatiety);
        var hoursDroppedSatietyTook = droppedSatiety / _levelSatietyDropPerHour;

        // when enough time has passed for +1 growth point
        if (hoursDroppedSatietyTook >= 1f / _growthPerHourWhenFed)
        {
            _currentGrowthPoints += (int)(hoursDroppedSatietyTook * _growthPerHourWhenFed);
            _currentSatiety -= droppedSatiety;
            _lastSatietyChange = DateTime.Now;
            GrowUsingGrowthPoints();
        }
    }

    private void GrowUsingGrowthPoints()
    {
        while (_currentGrowthPoints >= _levelGrowthPoints)
        {
            _currentGrowthPoints -= _levelGrowthPoints;
            _growthLevel++;
            _size += _sizeIncreasePerLevel;
            CalculateCurrentLevelAttributes();
        }
        transform.localScale = _size * Vector3.one;
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

    /// <summary>
    /// Calculate and assign attributes that change according to growth level
    /// </summary>
    private void CalculateCurrentLevelAttributes()
    {
        _levelGrowthPoints = _growthLevel * _baseGrowthPoints;
        _levelMaxSatiety = _baseMaxSatiety + (_growthLevel - 1) * _satietyIncreasePerLevel;
        _levelSatietyDropPerHour = _baseSatietyDropPerHour + (_growthLevel - 1) * _satietyDropIncreasePerLevel;
    }
}

public class FishData
{
    public int GrowthLevel;
    public int CurrentGrowthPoints;
    public float CurrentSatiety;
    public DateTime LastSatietyChange;
    public float Size;
}