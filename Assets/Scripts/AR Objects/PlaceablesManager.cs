using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class PlaceablesManager : Singleton<PlaceablesManager>
{
    [SerializeField] private Camera _camera;
    [SerializeField] private List<GameObject> _fishPrefabs;
    [SerializeField] private List<GameObject> _ornamentPrefabs;
    [SerializeField] private GameObject _geoObjectPrefab;
    public GameObject GetFishPrefabAtIndex(int index) => _fishPrefabs[index];
    public GameObject GetOrnamentPrefabAtIndex(int index) => _ornamentPrefabs[index];
    public GameObject OtherPrefab => _geoObjectPrefab;

    public int FishTanksCount => _geoObjects.Count(geoObj => geoObj is FishTank);

    private List<GeospatialObject> _geoObjects;
    private const string _storageKey = "FishTankStorage";
    private bool _storageResolved = false;

    // Start is called before the first frame update
    void Start()
    {
        _geoObjects = new();
        HandleGeospatialTrackingStateChanged(TrackingState.None);
        InteractionManager.Instance.ModifyOnChanged
            .AddListener(HandleSaveModifiedChanges);
        GeospatialManager.Instance.TrackingStateChanged
            .AddListener(HandleGeospatialTrackingStateChanged);
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

        foreach (var geoData in appData.GeospatialObjectDataList)
        {
            GeospatialObject geoObject = Instantiate(_geoObjectPrefab).GetComponent<GeospatialObject>();
            geoObject.Restore(geoData);
            if (!_geoObjects.Contains(geoObject))
            {
                _geoObjects.Add(geoObject);
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

    private void HandleGeospatialTrackingStateChanged(TrackingState newState)
    {
        if (_storageResolved)
            return;

        if (newState != TrackingState.Tracking)
        {
            StatusLog.Instance.DebugLog("Waiting for Geospatial before restoring objects");
            return;
        }
        if (PlayerPrefs.HasKey(_storageKey))
        {
            LoadSavedPlaceables();
            StatusLog.Instance.DebugLog($"PlayerPrefs restored: {_geoObjects.Count} GeoObjects");
            _storageResolved = true;
        }
        else
        {
            StatusLog.Instance.DebugLog("No data restored from PlayerPrefs");
        }
    }

    public void PlaceNewGeospatialObject()
    {
        //var geoObject = Instantiate(
        //    _geoObjectPrefab,
        //    _camera.transform.position + 1.5f * _camera.transform.forward,
        //    Quaternion.identity)
        //    .GetComponent<GeospatialObject>();
        //geoObject.Innit();
        //_geoObjects.Add(geoObject);

        //InteractionManager.Instance.SetModifyInvoke(true);
        //InteractionManager.Instance.SelectPlaceableGameObject(geoObject.gameObject);

        Instantiate(
            _geoObjectPrefab,
            _camera.transform.position + 1.5f * _camera.transform.forward,
            Quaternion.identity)
            .GetComponent<GeospatialObject>();
    }
}
