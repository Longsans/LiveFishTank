using UnityEngine;

public abstract class TankResident : MonoBehaviour, IVisibilityToggleable
{
    protected FishTank _tank;
    protected TankResidentData _saveData;
    public TankResidentData SaveData => _saveData;

    public virtual void Init(int prefabIndex, FishTank tank)
    {
        RegisterWithTank(tank);
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

    public virtual TankResidentData SaveAndReturnData()
    {
        Save();
        return _saveData;
    }

    public virtual void Restore(TankResidentData data, FishTank tank)
    {
        _saveData = data;
        RegisterWithTank(tank);
        transform.localPosition = _saveData.LocalPose.position;
        transform.localRotation = _saveData.LocalPose.rotation;
    }

    public void RegisterWithTank(FishTank tank)
    {
        _tank = tank;
        _tank.AddTankResident(this);
        transform.parent = _tank.transform;
    }

    public abstract void ToggleVisibility(bool visible);
}
