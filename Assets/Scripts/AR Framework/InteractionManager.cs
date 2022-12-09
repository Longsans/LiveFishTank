using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private Camera _camera;

    [HideInInspector] public UnityEvent ModifyOnChanged;
    [HideInInspector] public UnityEvent<GameObject> CurrentSelectedPlaceableChanged;
    [HideInInspector] public UnityEvent<PlaceableType> ObjectModeChanged;
    [HideInInspector] public UnityEvent SettingsMenuFinishedHide;

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
    private bool _settingsOpen = false;
    private const int _tankLayer = 7;

    public void NotifySettingsHidden()
    {
        SettingsMenuFinishedHide.Invoke();
    }

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
            if (ObjectMode != PlaceableType.Tank)
                return;

            var geoObject = RaycastForGeospatialObjectsFromScreenPos(touchPosition);
            if (geoObject)
                SelectPlaceableGameObject(geoObject.gameObject);
            else
            {
                SelectPlaceableGameObject(null);
            }
        }
        else
        {
            if (ObjectMode == PlaceableType.Tank)
                PlaceablesManager.Instance.PlaceNewGeospatialObject();
            else if (ObjectMode == PlaceableType.Fish || ObjectMode == PlaceableType.FishFood)
            {
                var geoObject = RaycastForGeospatialObjectsFromScreenPos(touchPosition);
                if (geoObject)
                    PlaceablesManager.Instance.PlaceNewLocalObject(geoObject);
                else
                    StatusLog.Instance.DebugLog("Raycast did not find any tank");
            }
        }
    }

    private GeospatialObject RaycastForGeospatialObjectsFromScreenPos(Vector2 screenPosition)
    {
        GeospatialObject geoObject;
        Ray ray = _camera.ScreenPointToRay(screenPosition);

        // if camera is looking at geo object
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << _tankLayer))
        {
            geoObject = hit.collider.GetComponentInParent<GeospatialObject>();
            return geoObject;
        }
        // if camera is inside a geo object, or not
        return PlaceablesManager.Instance.GetGeospatialObjectAtLocation(_camera.transform.position);
    }

    private void TryTogglePlaceableObjectBounds(GameObject placeable, bool show)
    {
        if (placeable &&
            placeable.TryGetComponent(out GeospatialObject geoObject))
            geoObject.ToggleBounds(show);
    }
}