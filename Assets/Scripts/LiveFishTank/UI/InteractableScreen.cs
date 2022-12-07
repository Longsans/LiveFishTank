using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARSubsystems;

public class InteractableScreen : MonoBehaviour, IPointerDownHandler
{
    void Start()
    {
        gameObject.SetActive(false);
        GeospatialManager.Instance.MinimumRequiredAccuracyReached
            .AddListener(HandleGeospatialRequiredAccuracyReached);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        InteractionManager.Instance.HandleScreenTouch(eventData.position);
    }

    private void HandleGeospatialRequiredAccuracyReached()
    {
        gameObject.SetActive(true);
    }
}
