using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;

[System.Serializable]
public class FishTankData
{
    public List<TankResidentData> TankResidentsDataList;
    public string OtherData;

    public FishTankData(
        List<TankResidentData> fishesDataList,
        string otherData = null)
    {
        TankResidentsDataList = fishesDataList;
        OtherData = otherData;
    }
}
