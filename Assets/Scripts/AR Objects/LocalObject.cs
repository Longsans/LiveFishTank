using UnityEngine;

public class LocalObject : PlaceableObject
{
    protected GeospatialObject _geoObject;
    protected LocalObjectData _saveData;
    public LocalObjectData SaveData => _saveData;

    public override void Innit(int prefabIndex)
    {
        _saveData = new(prefabIndex, new Pose(transform.position, transform.rotation));
    }

    /// <summary>
    /// Saves the <c>LocalObject</c>'s current data into <c>SaveData</c>
    /// </summary>
    public virtual void Save()
    {
        _saveData.LocalPose.position = transform.localPosition;
        _saveData.LocalPose.rotation = transform.localRotation;
    }

    public virtual void Restore(LocalObjectData localData)
    {
        _saveData = localData;
        transform.localPosition = _saveData.LocalPose.position;
        transform.localRotation = _saveData.LocalPose.rotation;
    }

    public void AnchorToGeospatialObject(GeospatialObject geoObject)
    {
        _geoObject = geoObject;
        _geoObject.RegisterLocalObject(this);
    }
}
