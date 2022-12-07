using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;

[System.Serializable]
public class GeospatialObjectData
{
    public double Latitude;
    public double Longitude;
    public double Altitude;
    public double Heading;
    public Vector3 PositionRelativeToGeoAnchor;
    public List<LocalObjectData> LocalObjectDataList;
    public string OtherData;

    public GeospatialObjectData(
        double latitude,
        double longitude,
        double altitude,
        double heading,
        Vector3 localPositionToGeoAnchor,
        List<LocalObjectData> localObjectDataList,
        string otherData = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
        Heading = heading;
        PositionRelativeToGeoAnchor = localPositionToGeoAnchor;
        LocalObjectDataList = localObjectDataList;
        OtherData = otherData;
    }
}
