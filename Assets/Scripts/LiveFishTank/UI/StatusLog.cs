using Google.XR.ARCoreExtensions;
using TMPro;
using UnityEngine;

public class StatusLog : Singleton<StatusLog>
{
    [SerializeField] private TMP_Text _statusLog;
    [SerializeField] private TMP_Text _geospatialLog;
    [SerializeField] private TMP_Text _debugLog;
    [SerializeField] private bool _logStackTrace;

    void Awake()
    {
        Application.logMessageReceived += (message, stackTrace, type) =>
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                _statusLog.text = $"Exception status: {message}{(_logStackTrace ? $"\nStack:\n{stackTrace}" : "")}";
            }
        };
    }

    public void DebugLog(string status)
    {
        _debugLog.text = $"Debug log:\n{status}";
    }

    public void UpdateGeospatialStatus(GeospatialPose geoPose)
    {
        _geospatialLog.text = "Geospatial status:\n" +
            $"Latitude: {geoPose.Latitude}\n" +
            $"Longitude: {geoPose.Longitude}\n" +
            $"Altitude: {geoPose.Altitude}\n" +
            $"Heading: {geoPose.Heading}";
    }

    public void LogGeospatialError(string error)
    {
        _geospatialLog.text = $"Geo pose unavailable.\nError: {error}";
    }
}
