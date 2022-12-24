using UnityEngine;

[System.Serializable]
public class TankResidentData
{
    public int PrefabIndex;
    public Pose LocalPose;
    public TankResidentType Type;
    public string OtherData;

    public TankResidentData(
        int prefabIndex,
        Pose localPose,
        TankResidentType type = TankResidentType.Other,
        string otherData = null)
    {
        PrefabIndex = prefabIndex;
        LocalPose = localPose;
        Type = type;
        OtherData = otherData;
    }
}
