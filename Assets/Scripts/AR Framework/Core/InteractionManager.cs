using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private Camera _camera;

    [HideInInspector] public UnityEvent ModifyingTankChanged;
    [HideInInspector] public UnityEvent<GameObject> CurrentSelectedPlaceableChanged;
    [HideInInspector] public UnityEvent<bool> IsPlacingTankChanged;
    [HideInInspector] public UnityEvent SettingsMenuFinishedHiding;

    public bool ModifyingTank { get; private set; } = false;
    public GameObject CurrentSelectedPlaceable => _currentSelectedPlaceable;
    public TankResidentType ResidentType { get; set; } = TankResidentType.Fish;
    private GameObject _currentSelectedPlaceable;
    private const int _fishTankLayer = 7;

    void Start()
    {
        Accelerometer.Instance.OnShake
            .AddListener(HandleScreenShake);
    }

    public void NotifySettingsHidden()
    {
        SettingsMenuFinishedHiding.Invoke();
    }

    public void SetModifyingTank(bool modifying)
    {
        ModifyingTank = modifying;
        if (!ModifyingTank)
            SelectPlaceableGameObject(null);
    }

    public void SetModifyingTankInvoke(bool modifying)
    {
        SetModifyingTank(modifying);
        ModifyingTankChanged?.Invoke();
    }

    public void SelectPlaceableGameObject(GameObject placeable)
    {
        var previousSelection = _currentSelectedPlaceable;
        TryTogglePlaceableObjectBounds(placeable, true);
        _currentSelectedPlaceable = placeable;
        TryTogglePlaceableObjectBounds(previousSelection, false);
        CurrentSelectedPlaceableChanged?.Invoke(_currentSelectedPlaceable);
    }

    public void HandleScreenTouch(Vector2 touchPosition)
    {
        var tankAtTouchPosition = RaycastForFishTankFromScreenPos(touchPosition);
        var tankBeneathDevice = RaycastForFishTank(new Ray(_camera.transform.position, Vector3.down));
        // tank needs to be underneath device, also with touch position raycast hitting it
        if (tankAtTouchPosition && tankBeneathDevice)
        {
            switch (ResidentType)
            {
                case TankResidentType.Fish:
                    PlaceablesManager.Instance.DropNewFishIntoTank();
                    break;
            }
        }
        else
            StatusLog.Instance.DebugLog("Please point your device at the fish tank");
    }

    private void HandleScreenShake()
    {
        if (ResidentType == TankResidentType.FishFood)
        {
            var tankBeneathDevice = RaycastForFishTank(new Ray(_camera.transform.position, Vector3.down));
            if (tankBeneathDevice)
                PlaceablesManager.Instance.DropNewFoodPieceIntoTank();
            else StatusLog.Instance.DebugLog("Please move your device directly above the tank and shake to drop food");
        }
    }

    private FishTank RaycastForFishTankFromScreenPos(Vector2 screenPosition)
    {
        Ray ray = _camera.ScreenPointToRay(screenPosition);
        return RaycastForFishTank(ray);
    }

    private FishTank RaycastForFishTank(Ray ray)
    {
        FishTank tank;
        // if camera is looking at fish tank
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << _fishTankLayer))
        {
            tank = hit.collider.GetComponentInParent<FishTank>();
            return tank;
        }
        // if camera is inside a fish tank, or not
        return PlaceablesManager.Instance.GetFishTankAtLocation(_camera.transform.position);
    }

    private void TryTogglePlaceableObjectBounds(GameObject placeable, bool show)
    {
        if (placeable &&
            placeable.TryGetComponent(out FishTank tank))
            tank.ToggleBounds(show);
    }
}