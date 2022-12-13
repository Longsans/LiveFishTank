using Google.XR.ARCoreExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GeospatialObject : MonoBehaviour
{
    public GeospatialObjectData SaveData => _saveData;
    public int LocalObjectsCount => _localObjects.Count;
    [HideInInspector] public BoxCollider Collider;

    protected List<LocalObject> _localObjects;
    protected GeospatialObjectData _saveData;
    protected Pose _anchoredWorldPose = new();
    protected bool _isInit;

    /// <summary>
    /// Option to "unhook" the Group from its Geospatial Anchor once we're at a set accuracy target.
    /// The target accuracy reached event is emitted by GeospatialManager.
    /// This effectively fixes the Group in space so there's no further drifting from Geoposition adjustments.
    /// </summary>
    [Tooltip("Detach from Geospatial anchors if accuracy target reached. Prevents continous drift.")]
    [SerializeField] protected bool _detachAtAccuracyTarget = true;

    public abstract void ToggleBounds(bool show);

    protected virtual void Awake()
    {
        _localObjects = new();
        Collider = GetComponentInChildren<BoxCollider>();
    }

    public virtual void Init()
    {
        _isInit = true;
        Save();
    }

    /// <summary>
    /// Saves the <c>GeospatialObject</c>'s current data into <c>SaveData</c>
    /// </summary>
    public virtual void Save()
    {
        var localObjectDataList = new List<LocalObjectData>();
        if (_localObjects.Count > 0)
        {
            localObjectDataList = _localObjects.Select(
            localObj =>
            {
                localObj.Save();
                return localObj.SaveData;
            }).ToList();
        }

        if (_isInit)
        {
            AttachToGeoAnchor(GeospatialManager.Instance.SessionGeoAnchor.transform);
            _saveData = new(
                GeospatialManager.Instance.SessionGeoPose.Latitude,
                GeospatialManager.Instance.SessionGeoPose.Longitude,
                GeospatialManager.Instance.SessionGeoPose.Altitude,
                GeospatialManager.Instance.SessionGeoPose.Heading,
                transform.localPosition,
                localObjectDataList);
            DetachFromGeoAnchor();
        }
        else
        {
            _saveData.PositionRelativeToGeoAnchor = transform.localPosition;
            _saveData.LocalObjectDataList = localObjectDataList;
        }
    }

    public virtual void Restore(GeospatialObjectData geoData, Transform anchor)
    {
        _isInit = false;
        _saveData = geoData;
        AttachToGeoAnchor(anchor);
        transform.localPosition = _saveData.PositionRelativeToGeoAnchor;
        DetachFromGeoAnchor();

        foreach (var localData in _saveData.LocalObjectDataList)
        {
            GameObject localObjPrefab = localData.Type switch
            {
                LocalObjectType.Fish => PlaceablesManager.Instance.GetFishPrefabAtIndex(localData.PrefabIndex),
                LocalObjectType.FishFood => PlaceablesManager.Instance.GetFishFoodGroupPrefabAtIndex(localData.PrefabIndex),
                LocalObjectType.Ornament => PlaceablesManager.Instance.GetOrnamentPrefabAtIndex(localData.PrefabIndex),
                _ => PlaceablesManager.Instance.OtherPrefab,
            };

            var localObj = Instantiate(localObjPrefab);
            var localObject = localObj.GetComponent<LocalObject>();

            localObject.Restore(localData, this);
        }
    }

    public void RegisterLocalObject(LocalObject localObject)
    {
        if (!_localObjects.Contains(localObject))
            _localObjects.Add(localObject);
    }

    public void DisposeLocalObject(LocalObject localObject)
    {
        if (_localObjects.Contains(localObject))
            _localObjects.Remove(localObject);
        Destroy(localObject.gameObject);
    }

    protected void AttachToGeoAnchor(Transform anchor)
    {
        transform.parent = anchor;
    }

    protected void DetachFromGeoAnchor()
    {
        transform.parent = null;
    }

    /// <summary>
    /// Clean up if needed
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (PlaceablesManager.Instance != null)
            PlaceablesManager.Instance.RemoveGeospatialObject(this);

        if (GeospatialManager.Instance != null)
        {
            GeospatialManager.Instance.MinimumRequiredAccuracyReached.RemoveListener(DetachFromGeoAnchor);
        }
    }
}
