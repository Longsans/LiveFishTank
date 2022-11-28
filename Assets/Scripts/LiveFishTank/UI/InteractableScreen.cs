using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARSubsystems;

public class InteractableScreen : MonoBehaviour, IPointerDownHandler
{
    void Start()
    {
        HandleGeospatialTrackingStateChanged(TrackingState.None);
        GeospatialManager.Instance.TrackingStateChanged.AddListener(HandleGeospatialTrackingStateChanged);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        InteractionManager.Instance.HandleScreenTouch(eventData.position);
    }

    private void HandleGeospatialTrackingStateChanged(TrackingState newState)
    {
        gameObject.SetActive(newState == TrackingState.Tracking);
    }
}
