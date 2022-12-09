using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARSubsystems;

public class PlaceablesManager : Singleton<PlaceablesManager>
{
    [SerializeField] private Camera _camera;
    [SerializeField] private List<GameObject> _fishPrefabs;
    [SerializeField] private List<GameObject> _fishFoodGroupPrefabs;
    [SerializeField] private List<GameObject> _ornamentPrefabs;
    [SerializeField] private GameObject _geoObjectPrefab;
    public GameObject GetFishPrefabAtIndex(int index) => _fishPrefabs[index];
    public GameObject GetOrnamentPrefabAtIndex(int index) => _ornamentPrefabs[index];
    public GameObject GetFishFoodGroupPrefabAtIndex(int index) => _fishFoodGroupPrefabs[index];
    public GameObject OtherPrefab => _geoObjectPrefab;
    public bool ShowGeospatialObjectsBounds
    {
        get => _visualizeGeoObjects;
        set
        {
            _visualizeGeoObjects = value;
            ShowGeospatialObjectsBoundsChanged.Invoke(_visualizeGeoObjects);
        }
    }

    public int FishTanksCount => _geoObjects.Count(geoObj => geoObj is FishTank);
    public int SelectedFishPrefabIndex { get; set; } = 0;
    public int SelectedFishFoodGroupPrefabIndex { get; set; } = 0;

    [HideInInspector] public UnityEvent<bool> ShowGeospatialObjectsBoundsChanged;

    private List<GeospatialObject> _geoObjects;
    private bool _visualizeGeoObjects = false;
    private const string _storageKey = "FishTankStorage";
    private bool _storageResolved = false;

    void Start()
    {
        _geoObjects = new();
        InteractionManager.Instance.ModifyOnChanged
            .AddListener(HandleSaveModifiedChanges);
        GeospatialManager.Instance.MinimumRequiredAccuracyReached
            .AddListener(HandleGeospatialRequiredAccuracyReached);
    }

    public GeospatialObject GetGeospatialObjectAtLocation(Vector3 pointInGeoObject)
    {
        foreach (var geoObject in _geoObjects)
        {
            if (geoObject.Collider.bounds.Contains(pointInGeoObject))
                return geoObject;
        }
        return null;
    }

    public void SavePlaceables()
    {
        List<GeospatialObjectData> geoDataList = _geoObjects.Select(
            geoObj =>
            {
                geoObj.Save();
                return geoObj.SaveData;
            }).ToList();
        AppData appData = new(geoDataList);

        PlayerPrefs.SetString(_storageKey, JsonUtility.ToJson(appData));
        PlayerPrefs.Save();
    }

    private void LoadSavedPlaceables()
    {
        AppData appData = JsonConvert.DeserializeObject<AppData>(
            PlayerPrefs.GetString(_storageKey));

        var sessions = appData.GeospatialObjectDataList.GroupBy(
            geoObject => new
            {
                geoObject.Latitude,
                geoObject.Longitude,
                geoObject.Altitude,
                geoObject.Heading
            });

        foreach (var s in sessions)
        {
            var sessionGeoAnchor = GeospatialManager.Instance.RequestGeospatialAnchor(
                s.Key.Latitude,
                s.Key.Longitude,
                s.Key.Altitude,
                s.Key.Heading
            );
            foreach (var geoData in s)
            {
                GeospatialObject geoObject = Instantiate(_geoObjectPrefab).GetComponent<GeospatialObject>();
                geoObject.Restore(geoData, sessionGeoAnchor.transform);
                if (!_geoObjects.Contains(geoObject))
                {
                    _geoObjects.Add(geoObject);
                }
            }
        }
    }

    private void HandleSaveModifiedChanges()
    {
        if (!InteractionManager.Instance.ModifyOn)
        {
            SavePlaceables();
            StatusLog.Instance.DebugLog("Saved objects to PlayerPrefs");
        }
    }

    private void HandleGeospatialRequiredAccuracyReached()
    {
        if (_storageResolved)
            return;

        if (PlayerPrefs.HasKey(_storageKey))
        {
            LoadSavedPlaceables();
            var localObjectsCount = _geoObjects.Sum(geoObj => geoObj.LocalObjectsCount);
            StatusLog.Instance.DebugLog($"PlayerPrefs restored: {FishTanksCount} GeoObjects - {localObjectsCount} LocalObjects total");
            _storageResolved = true;
        }
        else
        {
            StatusLog.Instance.DebugLog("No data restored from PlayerPrefs");
        }
    }

    public void PlaceNewGeospatialObject()
    {
        var geoObject = Instantiate(
            _geoObjectPrefab,
            _camera.transform.position + 1.5f * _camera.transform.forward,
            Quaternion.identity)
            .GetComponent<GeospatialObject>();
        geoObject.Init();
        _geoObjects.Add(geoObject);

        InteractionManager.Instance.SetModifyInvoke(true);
        InteractionManager.Instance.SelectPlaceableGameObject(geoObject.gameObject);

        // For testing without Location service
        //
        // Instantiate(
        //     _geoObjectPrefab,
        //     _camera.transform.position + 1.5f * _camera.transform.forward,
        //     Quaternion.identity)
        //     .GetComponent<GeospatialObject>();
    }

    public void PlaceNewLocalObject(GeospatialObject geoObject)
    {
        var placementPos = _camera.transform.position + 1.5f * _camera.transform.forward;
        if (!geoObject.Collider.bounds.Contains(placementPos))
        {
            StatusLog.Instance.DebugLog("Please place position the device a small distance away from the tank");
            return;
        }

        int prefabIndex;
        GameObject localObjectPrefab;
        if (InteractionManager.Instance.ObjectMode == PlaceableType.Fish)
        {
            prefabIndex = SelectedFishPrefabIndex;
            localObjectPrefab = _fishPrefabs[prefabIndex];
        }
        else
        {
            prefabIndex = SelectedFishFoodGroupPrefabIndex;
            localObjectPrefab = _fishFoodGroupPrefabs[prefabIndex];
        }

        var angle = Random.Range(30f, 180f);
        var localObject = Instantiate(
            localObjectPrefab,
            placementPos,
            Quaternion.AngleAxis(angle, Vector3.up))
            .GetComponent<LocalObject>();
        localObject.Init(prefabIndex, geoObject);
        SavePlaceables();
    }

    public void RemoveGeospatialObject(GeospatialObject geoObject)
    {
        if (_geoObjects.Contains(geoObject))
            _geoObjects.Remove(geoObject);
    }
}
