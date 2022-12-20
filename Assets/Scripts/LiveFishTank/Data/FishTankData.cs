using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;

[System.Serializable]
public class FishTankData
{
    public Vector3 Size;
    public List<TankResidentData> TankResidentsDataList;
    public string OtherData;

    public FishTankData(
        Vector3 size,
        List<TankResidentData> fishesDataList,
        string otherData = null)
    {
        Size = size;
        TankResidentsDataList = fishesDataList;
        OtherData = otherData;
    }
}
