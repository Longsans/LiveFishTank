using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GeospatialManager : Singleton<GeospatialManager>
{
    public GeospatialPose CameraGeospatialPose => _earthManager.CameraGeospatialPose;
    public bool GeospatialAvailable => _earthManager.EarthTrackingState == TrackingState.Tracking;
    [HideInInspector] public UnityEvent<TrackingState> TrackingStateChanged;

    [SerializeField] private AREarthManager _earthManager;
    [SerializeField] private ARAnchorManager _anchorManager;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _anchorPrefab;

    private TrackingState _lastTrackingState = TrackingState.None;

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

    public ARGeospatialAnchor RequestGeospatialAnchor()
    {
        return RequestGeospatialAnchor(_earthManager.CameraGeospatialPose);
    }

    public ARGeospatialAnchor RequestGeospatialAnchor(GeospatialPose pose)
    {
        Quaternion quaternion = Quaternion.AngleAxis(180f - (float)pose.Heading, Vector3.up);
        return _anchorManager.AddAnchor(pose.Latitude, pose.Longitude, pose.Altitude, quaternion);
    }
}
