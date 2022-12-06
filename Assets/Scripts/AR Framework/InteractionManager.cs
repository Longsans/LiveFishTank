using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ScaleSliders scaleSliders;

    [HideInInspector] public UnityEvent ModifyOnChanged;
    [HideInInspector] public UnityEvent<GameObject> CurrentSelectedPlaceableChanged;

    public bool ModifyOn { get; private set; } = false;
    public PlaceableType ObjectMode { get; set; } = PlaceableType.Fish;
    private GameObject _currentSelectedPlaceable;
    private const int _tankLayer = 7;

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
            if (ObjectMode == PlaceableType.Fish)
                return;

            Ray ray = _camera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << _tankLayer))
            {
                SelectPlaceableGameObject(hit.collider.transform.parent.gameObject);
            }
            else
            {
                SelectPlaceableGameObject(null);
            }
        }
        else
        {
            if (ObjectMode == PlaceableType.Fish)
            {
                Ray ray = _camera.ScreenPointToRay(touchPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << _tankLayer))
                {
                    var geoObject = hit.collider.GetComponentInParent<GeospatialObject>();
                    PlaceablesManager.Instance.PlaceNewLocalObject(geoObject);
                }
                else
                    StatusLog.Instance.DebugLog("Raycast did not find any tank");
            }
            else
                PlaceablesManager.Instance.PlaceNewGeospatialObject();
        }
    }
}