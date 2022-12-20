using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARManager : Singleton<ARManager>
{
    [SerializeField] private ARPlaneManager _planeManager;

    void Start()
    {
        PlaceablesManager.Instance.StartedPlacingTank
            .AddListener(OnStartedPlacingTank);
        PlaceablesManager.Instance.FinishedPlacingTank
            .AddListener(OnFinishedPlacingTank);
    }

    private void OnStartedPlacingTank()
    {
        foreach (var plane in _planeManager.trackables)
            plane.gameObject.SetActive(true);
    }

    private void OnFinishedPlacingTank()
    {
        foreach (var plane in _planeManager.trackables)
            plane.gameObject.SetActive(false);
    }
}
