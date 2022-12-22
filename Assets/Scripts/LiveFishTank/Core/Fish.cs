using System;
using UnityEngine;
using UnityEngine.Events;

public class Fish : TankResident
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
    [SerializeField] private float _swimSpeed = 0.1f;
    [SerializeField] private Transform _fishMouth;
    [SerializeField] private Canvas _fishUI;

    private readonly Vector3 _baseScale = 0.3f * Vector3.one;
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
    private Rigidbody _rigidbody;

    public FishState State { get; set; }

    void Awake()
    {
        _rigidbody = GetComponentInChildren<Rigidbody>();
    }

    void FixedUpdate()
    {
        Debug.Log($"current state: {State}");
        State.FixedUpdate();
    }

    void Update()
    {
        State.Update();
        GrowAccordingToRemainingSatiety();
    }

    public override void Init(int prefabIndex, FishTank tank)
    {
        base.Init(prefabIndex, tank);
        SetUpWithTank();
        CalculateCurrentLevelAttributes();
        TogglePhysics(true);
        State = new DroppingIntoTankState(this);
        _lastSatietyChange = DateTime.Now;
        _saveData.Type = TankResidentType.Fish;
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

    public override void Restore(TankResidentData localData, FishTank geoObject)
    {
        base.Restore(localData, geoObject);
        SetUpWithTank();
        TogglePhysics(false);
        var fishData = JsonUtility.FromJson<FishData>(localData.OtherData);
        _growthLevel = fishData.GrowthLevel;
        _currentGrowthPoints = fishData.CurrentGrowthPoints;
        _currentSatiety = fishData.CurrentSatiety;
        _lastSatietyChange = fishData.LastSatietyChange;
        _size = fishData.Size;
        State = IsFull ? new WanderingAndFullState(this) : new WanderingAndHungryState(this);

        CalculateCurrentLevelAttributes();
        GrowAccordingToRemainingSatiety();
        transform.localScale *= _size;
        AllDataInitialized.Invoke();
    }

    public void TogglePhysics(bool physicsOn)
    {
        var rb = GetComponentInChildren<Rigidbody>();
        _fishUI.enabled = !physicsOn;
        rb.useGravity = physicsOn;
    }

    public FishFood DetectFood()
    {
        _foodHeadedFor = _tank.GetNextFishFoodInTank();
        return _foodHeadedFor;
    }

    public void SwimAroundInTank()
    {
        // fish prefab model's actual forward-facing direction is its -X axis
        _rigidbody.velocity = -transform.right * _swimSpeed;
    }

    public void SwimToFood()
    {
        Vector3 distance = _foodHeadedFor.transform.position - _fishMouth.position;
        _rigidbody.velocity = distance.normalized * _swimSpeed;
    }

    public override void ToggleVisibility(bool visible)
    {
        var renderer = GetComponentInChildren<Renderer>();
        renderer.enabled = _fishUI.enabled = visible;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<FishFood>(out var food))
        {
            State.HandleCollideWithFood(food);
        }
    }

    void OnTriggerStay(Collider other)
    {

        if (other.CompareTag(FishTank.WaterTag))
        {
            State.HandleCollidedWithWater();
        }
    }

    public void ConsumeFood(FishFood food)
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
        transform.localScale = _size * _baseScale;
    }

    private void ResetToNormalState()
    {
        _foodHeadedFor = null;
        transform.rotation *= Quaternion.AngleAxis(-transform.rotation.eulerAngles.z, Vector3.forward);
        State.OnFoodConsumed();
    }

    private void HandleFoodPieceConsumed(FishFood consumedFood)
    {
        if (consumedFood == _foodHeadedFor)
            ResetToNormalState();
    }

    private void SetUpWithTank()
    {
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