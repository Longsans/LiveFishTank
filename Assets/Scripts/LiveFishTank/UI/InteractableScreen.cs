using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARSubsystems;

public class InteractableScreen : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        InteractionManager.Instance.HandleScreenTouch(eventData.position);
    }
}
