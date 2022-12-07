using UnityEngine;

[System.Serializable]
public class LocalObjectData
{
    public int PrefabIndex;
    public Pose LocalPose;
    public LocalObjectType Type;
    public string OtherData;

    public LocalObjectData(
        int prefabIndex,
        Pose localPose,
        LocalObjectType type = LocalObjectType.Other,
        string otherData = null)
    {
        PrefabIndex = prefabIndex;
        LocalPose = localPose;
        Type = type;
        OtherData = otherData;
    }
}
