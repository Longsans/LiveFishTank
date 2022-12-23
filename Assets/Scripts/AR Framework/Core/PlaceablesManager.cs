using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class PlaceablesManager : Singleton<PlaceablesManager>
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ARRaycastManager _raycastManager;
    [SerializeField] private ARPlaneManager _planeManager;

    [SerializeField]
    private List<GameObject> _fishPrefabs,
                            _fishFoodPrefabs,
                            _ornamentPrefabs;

    [SerializeField]
    private GameObject _tankPrefab,
                        _tankInPlacingPrefab;

    public GameObject GetFishPrefabAtIndex(int index) => _fishPrefabs[index];
    public GameObject GetOrnamentPrefabAtIndex(int index) => _ornamentPrefabs[index];
    public GameObject GetFishFoodPrefabAtIndex(int index) => _fishFoodPrefabs[index];
    public GameObject OtherPrefab => _tankPrefab;

    public int SelectedFishPrefabIndex { get; set; } = 0;
    public int SelectedFishFoodPrefabIndex { get; set; } = 0;
    public bool IsPlacingTank => _isPlacingTank;
    public bool TankPlaceable { get; set; } = false;
    public FishTank Tank => _tank;
    public bool TankPlaced => _tank.Visible;

    [HideInInspector] public UnityEvent StartedPlacingTank;
    [HideInInspector] public UnityEvent FinishedPlacingTank;

    private FishTank _tank;
    private Transform _tankTransformPriorToEnterPlacing;
    private bool _isPlacingTank = false;
    private const string _storageKey = "FishTankStorage";

    void Start()
    {
        InteractionManager
            .Instance
            .ModifyingTankChanged
                .AddListener(SaveModifiedChanges);
        LoadSavedPlaceables();
    }

    void Update()
    {
        if (!_isPlacingTank)
            return;
        if (TryPlaceTankOnDetectedPlane() && !TankPlaced)
            _tank.ToggleVisibility(true);
    }

    public FishTank GetFishTankAtLocation(Vector3 pointInTank)
    {
        if (_tank.TankCollider.bounds.Contains(pointInTank))
            return _tank;
        return null;
    }

    public void SavePlaceables()
    {
        var appData = _tank.SaveAndReturnData();

        PlayerPrefs.SetString(_storageKey, JsonUtility.ToJson(appData));
        PlayerPrefs.Save();
    }

    private void LoadSavedPlaceables()
    {
        _tank = Instantiate(
            _tankPrefab,
            _camera.transform.position + _camera.transform.forward,
            Quaternion.AngleAxis(_camera.transform.rotation.eulerAngles.y, Vector3.up))
            .GetComponent<FishTank>();
        _tank.ToggleVisibility(false);

        if (!PlayerPrefs.HasKey(_storageKey))
        {
            StatusLog.Instance.DebugLog("No previous session data");
            return;
        }
        FishTankData appData = JsonConvert.DeserializeObject<FishTankData>(
            PlayerPrefs.GetString(_storageKey));

        _tank.Restore(appData);
    }

    private void SaveModifiedChanges()
    {
        if (!InteractionManager.Instance.ModifyingTank)
        {
            SavePlaceables();
            StatusLog.Instance.DebugLog("Saved objects to PlayerPrefs");
        }
    }

    public void DropNewFishIntoTank()
    {
        var angle = Random.Range(30f, 180f);
        var fish = Instantiate(
            _fishPrefabs[SelectedFishPrefabIndex],
            _camera.transform.position + 0.25f * _camera.transform.forward,
            Quaternion.AngleAxis(angle, Vector3.up))
            .GetComponent<TankResident>();
        fish.Init(SelectedFishPrefabIndex, _tank);
        _tank.TankCollider.enabled = false;
    }

    public void DropNewFoodPieceIntoTank()
    {
        var positionDeviation = new Vector3(
            Random.Range(-0.05f, 0.05f),
            Random.Range(0.2f, 0.3f),
            Random.Range(-0.05f, 0.05f)
        );
        var fishFoodGroup = Instantiate(
            _fishFoodPrefabs[SelectedFishFoodPrefabIndex],
            _camera.transform.position + positionDeviation,
            Quaternion.identity)
            .GetComponent<TankResident>();
        fishFoodGroup.Init(SelectedFishFoodPrefabIndex, _tank);
        _tank.TankCollider.enabled = false;
    }

    public void PlaceNewOrnamentInTank()
    {
        // TODO: FIGURE OUT ORNAMENT PLACEMENT INTERACTION
        //
    }

    private bool TryPlaceTankOnDetectedPlane()
    {
        var middleOfScreen = _camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        if (!_raycastManager.Raycast(middleOfScreen, hits, TrackableType.Planes))
        {
            StatusLog
                .Instance
                .DebugLog("Please point your device at a flat horizontal surface, e.g. floor, table surface, to place the tank");
            return false;
        }

        var plane = _planeManager.GetPlane(hits[0].trackableId);
        // "plane alignment horizontal up" means upward-facing surfaces like the floor, "horizontal down" would be ceiling
        if (plane.alignment != PlaneAlignment.HorizontalUp)
        {
            StatusLog
                .Instance
                .DebugLog("Please point your device at a flat horizontal surface, e.g. floor, table surface, to place the tank");
            return false;
        }

        _tank.transform.position = hits[0].pose.position;
        TankPlaceable = true;
        return true;
    }

    public void StartTankPlacement()
    {
        if (_isPlacingTank)
            return;

        _isPlacingTank = true;
        _tank.TankCollider.enabled = true;
        if (!TankPlaced)
        {
            if (TryPlaceTankOnDetectedPlane())
                _tank.ToggleVisibility(true);
        }
        else
        {
            Debug.Log("tank transform prior to placing assigned");
            _tankTransformPriorToEnterPlacing =
                Instantiate(
                    _tankInPlacingPrefab,
                    _tank.transform.position,
                    _tank.transform.rotation)
                .GetComponent<Transform>();
        }
        StartedPlacingTank.Invoke();
        TankPlaceable = false;
    }

    public void ConfirmTankPlacement()
    {
        FinishTankPlacement();
        SavePlaceables();
    }

    public void CancelTankPlacement()
    {
        Debug.Log($"is placing tank: {_isPlacingTank}");
        if (!_isPlacingTank)
            return;
        if (_tankTransformPriorToEnterPlacing)
            _tank.transform.position = _tankTransformPriorToEnterPlacing.position;
        else
        {
            Debug.Log("tank transform null on cancel");
            _tank.ToggleVisibility(false);
        }
        FinishTankPlacement();
    }

    private void FinishTankPlacement()
    {
        _isPlacingTank = false;
        if (_tankTransformPriorToEnterPlacing)
            Destroy(_tankTransformPriorToEnterPlacing.gameObject);
        FinishedPlacingTank.Invoke();
    }
}
