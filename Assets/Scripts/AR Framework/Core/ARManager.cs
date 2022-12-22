using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARManager : Singleton<ARManager>
{
    [SerializeField] private ARPlaneManager _planeManager;

    void Start()
    {
        _planeManager.enabled = false;
        PlaceablesManager.Instance.StartedPlacingTank
            .AddListener(OnStartedPlacingTank);
        PlaceablesManager.Instance.FinishedPlacingTank
            .AddListener(OnFinishedPlacingTank);
    }

    private void OnStartedPlacingTank()
    {
        _planeManager.enabled = true;
        foreach (var plane in _planeManager.trackables)
            plane.gameObject.SetActive(true);
    }

    private void OnFinishedPlacingTank()
    {
        _planeManager.enabled = false;
        foreach (var plane in _planeManager.trackables)
            plane.gameObject.SetActive(false);
    }
}
