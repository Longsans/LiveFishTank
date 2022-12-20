using System.Collections.Generic;

[System.Serializable]
public class AppData
{
    public List<FishTankData> FishTanksDataList;

    public AppData(List<FishTankData> fishTanksDataList)
    {
        FishTanksDataList = fishTanksDataList;
    }
}
