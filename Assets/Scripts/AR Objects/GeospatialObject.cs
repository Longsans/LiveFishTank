using Google.XR.ARCoreExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeospatialObject : PlaceableObject
{
    public GeospatialObjectData SaveData => _saveData;

    protected List<LocalObject> _localObjects;
    protected GeospatialObjectData _saveData;

    public override void Innit(int prefabIndex = 0)
    {
        _localObjects = new();
        AttachToGeoAnchor(GeospatialManager.Instance.RequestGeospatialAnchor());
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

        _saveData = new(
            GeospatialManager.Instance.CameraGeospatialPose.Latitude,
            GeospatialManager.Instance.CameraGeospatialPose.Longitude,
            GeospatialManager.Instance.CameraGeospatialPose.Altitude,
            GeospatialManager.Instance.CameraGeospatialPose.Heading,
            new Vector3(
                transform.localPosition.x,
                transform.localPosition.y,
                transform.localPosition.z),
            localObjectDataList);
    }

    public virtual void Restore(GeospatialObjectData geoData)
    {
        _saveData = geoData;
        GeospatialPose geoPose = new()
        {
            Latitude = _saveData.Latitude,
            Longitude = _saveData.Longitude,
            Altitude = _saveData.Altitude,
            Heading = _saveData.Heading,
        };
        AttachToGeoAnchor(GeospatialManager.Instance.RequestGeospatialAnchor(geoPose));
        transform.localPosition = _saveData.PositionRelativeToGeoAnchor;

        _localObjects = new List<LocalObject>();
        foreach (var localData in _saveData.LocalObjectDataList)
        {
            GameObject localObjPrefab = localData.Type switch
            {
                LocalObjectType.Fish => PlaceablesManager.Instance.GetFishPrefabAtIndex(localData.PrefabIndex),
                LocalObjectType.Ornament => PlaceablesManager.Instance.GetOrnamentPrefabAtIndex(localData.PrefabIndex),
                _ => PlaceablesManager.Instance.OtherPrefab,
            };
            LocalObject localObject =
                Instantiate(localObjPrefab)
                    .GetComponent<LocalObject>();

            localObject.Restore(localData);
            if (!_localObjects.Contains(localObject))
            {
                _localObjects.Add(localObject);
            }
        }
    }

    public void RegisterLocalObject(LocalObject localObject)
    {
        if (!_localObjects.Contains(localObject))
            _localObjects.Add(localObject);
    }

    public void RemoveLocalObject(LocalObject localObject)
    {
        if (_localObjects.Contains(localObject))
            _localObjects.Remove(localObject);
    }

    protected virtual void AttachToGeoAnchor(ARGeospatialAnchor anchor)
    {
        transform.parent = anchor.transform;
    }
}
