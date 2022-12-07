using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GeospatialManager : Singleton<GeospatialManager>
{
    public bool GeospatialAvailable => _earthManager.EarthTrackingState == TrackingState.Tracking;
    public bool IsRequiredAccuracyReached => _requiredAccuracyReached;
    [HideInInspector] public ARGeospatialAnchor SessionGeoAnchor => _sessionGeoAnchor ??= CreateSessionGeoAnchor();
    [HideInInspector] public GeospatialPose SessionGeoPose { get; private set; }

    [HideInInspector] public UnityEvent AccuracyImproved;
    [HideInInspector] public UnityEvent MinimumRequiredAccuracyReached;

    [SerializeField] private AREarthManager _earthManager;
    [SerializeField] private ARAnchorManager _anchorManager;

    [Header("[ Accuracy Targets ] - Event raised when reached")]
    [SerializeField] private float _requiredHorizontalAccuracy = 1;
    [SerializeField] private float _requiredHeadingAccuracy = 2;
    [SerializeField] private float _requiredVerticalAccuracy = 0.5f;

    private ARGeospatialAnchor _sessionGeoAnchor;
    private double _bestHorizontalAccuracy = Mathf.Infinity;
    private double _bestHeadingAccuracy = Mathf.Infinity;
    private double _bestVerticalAccuracy = Mathf.Infinity;
    private bool _requiredAccuracyReached = false;

    void Start()
    {
        if (_earthManager.EarthTrackingState != TrackingState.Tracking)
            StatusLog.Instance.DebugLog("Waiting for Geospatial to start up");
    }

    void Update()
    {
        if (_earthManager.EarthTrackingState == TrackingState.Tracking)
        {
            StatusLog.Instance.UpdateGeospatialStatus(_earthManager.CameraGeospatialPose);
            if (CheckAccuracyImproved())
            {
                if (!_requiredAccuracyReached)
                {
                    StatusLog.Instance.DebugLog(
                        "Move your device around to improve Geospatial accuracy.\n" +
                        "Minimum required accuracy: " +
                        $"{_requiredHorizontalAccuracy} - {_requiredVerticalAccuracy} - {_requiredHeadingAccuracy}\n" +
                        "Current accuracy (lower is better): " +
                        $"{_bestHorizontalAccuracy} - {_bestVerticalAccuracy} - {_bestHeadingAccuracy}");
                }
                AccuracyImproved.Invoke();
            }
            if (!_requiredAccuracyReached && CheckRequiredAccuracyReached())
            {
                StatusLog.Instance.DebugLog("Accuracy target reached.");
                MinimumRequiredAccuracyReached.Invoke();
                _requiredAccuracyReached = true;
            }
        }
        else
        {
            if (_earthManager.EarthState == EarthState.Enabled)
                return;

            string error = $"{_earthManager.EarthState}\n";
            if (_earthManager.EarthState == EarthState.ErrorInternal)
                error += "^ This error cannot be resolved. Please restart the application.";

            StatusLog.Instance.LogGeospatialError(error);
        }
    }

    public ARGeospatialAnchor RequestGeospatialAnchor(GeospatialPose pose)
    {
        Quaternion quaternion = Quaternion.AngleAxis(180f - (float)pose.Heading, Vector3.up);
        return _anchorManager.AddAnchor(pose.Latitude, pose.Longitude, pose.Altitude, quaternion);
    }

    public ARGeospatialAnchor RequestGeospatialAnchor(double lat, double lng, double alt, double heading)
    {
        Quaternion quaternion = Quaternion.AngleAxis(180f - (float)heading, Vector3.up);
        return _anchorManager.AddAnchor(lat, lng, alt, quaternion);
    }

    public ARGeospatialAnchor CreateSessionGeoAnchor()
    {
        SessionGeoPose = _earthManager.CameraGeospatialPose;
        return RequestGeospatialAnchor(SessionGeoPose);
    }

    private bool CheckAccuracyImproved()
    {
        bool horizontal = _earthManager.CameraGeospatialPose.HorizontalAccuracy < _bestHorizontalAccuracy;
        bool heading = _earthManager.CameraGeospatialPose.HeadingAccuracy < _bestHeadingAccuracy;
        bool vertical = _earthManager.CameraGeospatialPose.VerticalAccuracy < _bestVerticalAccuracy;

        bool improved = false;

        if (horizontal)
        {
            improved = true;
            _bestHorizontalAccuracy = _earthManager.CameraGeospatialPose.HorizontalAccuracy;
        }
        if (heading)
        {
            improved = true;
            _bestHeadingAccuracy = _earthManager.CameraGeospatialPose.HeadingAccuracy;
        }
        if (vertical)
        {
            improved = true;
            _bestVerticalAccuracy = _earthManager.CameraGeospatialPose.VerticalAccuracy;
        }

        return improved;
    }

    private bool CheckRequiredAccuracyReached()
    {
        return _earthManager.CameraGeospatialPose.HorizontalAccuracy <= _requiredHorizontalAccuracy &&
               _earthManager.CameraGeospatialPose.HeadingAccuracy <= _requiredHeadingAccuracy &&
               _earthManager.CameraGeospatialPose.VerticalAccuracy <= _requiredVerticalAccuracy;
    }
}
