using UnityEngine;

public class LocalObject : MonoBehaviour
{
    protected GeospatialObject _geoObject;
    protected LocalObjectData _saveData;
    public LocalObjectData SaveData => _saveData;

    public virtual void Init(int prefabIndex, GeospatialObject geoObject)
    {
        _saveData = new(prefabIndex, new Pose(transform.position, transform.rotation));
        AnchorToGeospatialObject(geoObject);
    }

    /// <summary>
    /// Saves the <c>LocalObject</c>'s current data into <c>SaveData</c>
    /// </summary>
    public virtual void Save()
    {
        _saveData.LocalPose.position = transform.localPosition;
        _saveData.LocalPose.rotation = transform.localRotation;
    }

    public virtual void Restore(LocalObjectData localData, GeospatialObject geoObject)
    {
        _saveData = localData;
        AnchorToGeospatialObject(geoObject);
        transform.localPosition = _saveData.LocalPose.position;
        transform.localRotation = _saveData.LocalPose.rotation;
    }

    public void AnchorToGeospatialObject(GeospatialObject geoObject)
    {
        _geoObject = geoObject;
        _geoObject.RegisterLocalObject(this);
        transform.parent = _geoObject.transform;
    }
}
