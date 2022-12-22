using Google.XR.ARCoreExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class StatusLog : Singleton<StatusLog>
{
    [SerializeField] private TMP_Text _statusLog;
    [SerializeField] private TMP_Text _debugLog;
    [SerializeField] private bool _logStackTrace;

    void Awake()
    {
        Application.logMessageReceived += (message, stackTrace, type) =>
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                _statusLog.text = $"Exception status: {message}";
                if (_logStackTrace)
                    _debugLog.text = $"{stackTrace}";
            }
        };
    }

    public void DebugLog(string status)
    {
        _debugLog.text = $"Debug log:\n{status}";
    }

    public void DebugLogAppend(string status)
    {
        _debugLog.text += $"\n{status}";
    }

    public void UpdateGeospatialStatus(TrackingState state)
    {

    }

    public void LogGeospatialError(string error)
    {

    }
}
