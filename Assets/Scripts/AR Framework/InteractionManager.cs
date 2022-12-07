using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ScaleSliders scaleSliders;

    [HideInInspector] public UnityEvent ModifyOnChanged;
    [HideInInspector] public UnityEvent<GameObject> CurrentSelectedPlaceableChanged;
    [HideInInspector] public UnityEvent<PlaceableType> ObjectModeChanged;

    public bool ModifyOn { get; private set; } = false;
    public GameObject CurrentSelectedPlaceable => _currentSelectedPlaceable;
    public PlaceableType ObjectMode
    {
        get => _objectMode;
        set
        {
            _objectMode = value;
            ObjectModeChanged.Invoke(_objectMode);
        }
    }
    private PlaceableType _objectMode = PlaceableType.Fish;
    private GameObject _currentSelectedPlaceable;
    private const int _tankLayer = 7;

    public void SetModify(bool modifyOn)
    {
        ModifyOn = modifyOn;
        if (!ModifyOn)
            SelectPlaceableGameObject(null);
    }

    public void SetModifyInvoke(bool modifyOn)
    {
        SetModify(modifyOn);
        ModifyOnChanged?.Invoke();
    }

    public void SelectPlaceableGameObject(GameObject placeable)
    {
        var previousSelection = _currentSelectedPlaceable;
        TryTogglePlaceableObjectBounds(placeable, true);
        _currentSelectedPlaceable = placeable;
        TryTogglePlaceableObjectBounds(
            previousSelection,
            PlaceablesManager.Instance.ShowGeospatialObjectsBounds);
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

            var geoObject = RaycastForGeospatialObjectsFromScreenPos(touchPosition);
            if (geoObject)
                SelectPlaceableGameObject(geoObject.gameObject);
            else
                SelectPlaceableGameObject(null);
        }
        else
        {
            if (ObjectMode == PlaceableType.Fish)
            {
                var geoObject = RaycastForGeospatialObjectsFromScreenPos(touchPosition);
                if (geoObject)
                    PlaceablesManager.Instance.PlaceNewLocalObject(geoObject);
                else
                    StatusLog.Instance.DebugLog("Raycast did not find any tank");
            }
            else
                PlaceablesManager.Instance.PlaceNewGeospatialObject();
        }
    }

    private GeospatialObject RaycastForGeospatialObjectsFromScreenPos(Vector2 screenPosition)
    {
        Ray ray = _camera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << _tankLayer))
        {
            var geoObject = hit.collider.GetComponentInParent<GeospatialObject>();
            return geoObject;
        }
        return null;
    }

    private void TryTogglePlaceableObjectBounds(GameObject placeable, bool show)
    {
        if (placeable &&
            placeable.TryGetComponent(out GeospatialObject geoObject))
            geoObject.ToggleBounds(show);
    }
}