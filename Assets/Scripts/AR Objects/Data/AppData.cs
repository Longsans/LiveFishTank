using System.Collections.Generic;

[System.Serializable]
public class AppData
{
    public List<GeospatialObjectData> GeospatialObjectDataList;

    public AppData(List<GeospatialObjectData> geospatialObjectDataList)
    {
        GeospatialObjectDataList = geospatialObjectDataList;
    }
}
