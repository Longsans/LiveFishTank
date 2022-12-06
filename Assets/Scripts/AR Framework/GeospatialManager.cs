using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GeospatialManager : Singleton<GeospatialManager>
{
    public GeospatialPose CameraGeospatialPose => _earthManager.CameraGeospatialPose;
    public bool GeospatialAvailable => _earthManager.EarthTrackingState == TrackingState.Tracking;
    public bool IsAccuracyTargetReached => _targetAccuracyReached;
    [HideInInspector] public ARGeospatialAnchor SessionGeoAnchor => _sessionGeoAnchor ??= CreateSessionGeoAnchor();
    [HideInInspector] public GeospatialPose SessionGeoPose { get; private set; }

    [HideInInspector] public UnityEvent<TrackingState> TrackingStateChanged;
    [HideInInspector] public UnityEvent AccuracyImproved;
    [HideInInspector] public UnityEvent TargetAccuracyReached;

    [SerializeField] private AREarthManager _earthManager;
    [SerializeField] private ARAnchorManager _anchorManager;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _anchorPrefab;

    [Header("[ Accuracy Minimums ] - Required to start experience")]
    [SerializeField] private float _minimumHorizontalAccuracy = 10;
    [SerializeField] private float _minimumHeadingAccuracy = 15;
    [SerializeField] private float _minimumVerticalAccuracy = 1.5f;

    [Header("[ Accuracy Targets ] - Event raised when reached")]
    [SerializeField] private float _targetHorizontalAccuracy = 1;
    [SerializeField] private float _targetHeadingAccuracy = 2;
    [SerializeField] private float _targetVerticalAccuracy = 0.5f;

    private TrackingState _lastTrackingState = TrackingState.None;
    private ARGeospatialAnchor _sessionGeoAnchor;
    private double _bestHorizontalAccuracy = Mathf.Infinity;
    private double _bestHeadingAccuracy = Mathf.Infinity;
    private double _bestVerticalAccuracy = Mathf.Infinity;
    private bool _targetAccuracyReached = false;

    void Update()
    {
        if (_lastTrackingState != _earthManager.EarthTrackingState)
        {
            TrackingStateChanged?.Invoke(_earthManager.EarthTrackingState);
            _lastTrackingState = _earthManager.EarthTrackingState;
        }

        if (_earthManager.EarthTrackingState == TrackingState.Tracking)
        {
            StatusLog.Instance.UpdateGeospatialStatus(_earthManager.CameraGeospatialPose);
            if (CheckAccuracyImproved())
            {
                StatusLog.Instance.DebugLog(
                    $"Geospatial accuracy improved: {_bestHorizontalAccuracy} - {_bestVerticalAccuracy} - {_bestHeadingAccuracy}");

                AccuracyImproved.Invoke();
            }
            if (!_targetAccuracyReached && CheckTargetAccuracyReached())
            {
                StatusLog.Instance.DebugLog("Accuracy target reached");
                TargetAccuracyReached.Invoke();
                _targetAccuracyReached = true;
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

    /// <summary>
    /// Compare current tracking accuracy against best values.
    /// Return whether or not accuracy has improved since the last check.
    /// </summary>
    /// <returns></returns>
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

    private bool CheckTargetAccuracyReached()
    {
        return _earthManager.CameraGeospatialPose.HorizontalAccuracy <= _targetHorizontalAccuracy &&
               _earthManager.CameraGeospatialPose.HeadingAccuracy <= _targetHeadingAccuracy &&
               _earthManager.CameraGeospatialPose.VerticalAccuracy <= _targetVerticalAccuracy;
    }
}
