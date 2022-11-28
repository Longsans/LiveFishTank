using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ScaleSliders scaleSliders;

    [HideInInspector] public UnityEvent ModifyOnChanged;
    [HideInInspector] public UnityEvent<GameObject> CurrentSelectedPlaceableChanged;

    public bool ModifyOn { get; private set; } = false;
    private GameObject _currentSelectedPlaceable;

    public void SetModify(bool modifyOn)
    {
        ModifyOn = modifyOn;
        if (!ModifyOn)
            scaleSliders.gameObject.SetActive(false);
    }

    public void SetModifyInvoke(bool modifyOn)
    {
        SetModify(modifyOn);
        ModifyOnChanged?.Invoke();
    }

    public void SelectPlaceableGameObject(GameObject placeable)
    {
        _currentSelectedPlaceable = placeable;
        CurrentSelectedPlaceableChanged?.Invoke(_currentSelectedPlaceable);
    }

    public void HandleScreenTouch(Vector2 touchPosition)
    {
        if (!GeospatialManager.Instance.GeospatialAvailable)
            return;

        if (ModifyOn)
        {
            Ray ray = _camera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SelectPlaceableGameObject(hit.collider.gameObject);
            }
            else
            {
                SelectPlaceableGameObject(null);
            }
        }
        else
        {
            PlaceablesManager.Instance.PlaceNewGeospatialObject();
        }
    }
}
