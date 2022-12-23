using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private Camera _camera;
    [HideInInspector] public UnityEvent ModifyingTankChanged;
    [HideInInspector] public UnityEvent SettingsMenuFinishedHiding;

    public bool ModifyingTank { get; private set; } = false;
    public GameObject CurrentSelectedPlaceable => _currentSelectedPlaceable;

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
    }

    public void HandleScreenTouch(Vector2 touchPosition)
    {
        if (PlaceablesManager.Instance.ResidentType != TankResidentType.Fish)
            return;

        var tankAtTouchPosition = RaycastForFishTankFromScreenPos(touchPosition);
        var tankBeneathDevice = RaycastForFishTank(new Ray(_camera.transform.position, Vector3.down));
        // tank needs to be underneath device, also with touch position raycast hitting it
        if (tankAtTouchPosition && tankBeneathDevice)
        {
            StatusLog.Instance.DebugLog("Fish dropped");
            PlaceablesManager.Instance.DropNewFishIntoTank();
        }
        else
            StatusLog.Instance.DebugLog("Please point your device at the fish tank while standing above it");
    }

    private void HandleScreenShake()
    {
        if (PlaceablesManager.Instance.ResidentType != TankResidentType.FishFood)
            return;

        var tankBeneathDevice = RaycastForFishTank(new Ray(_camera.transform.position, Vector3.down));
        if (tankBeneathDevice)
            PlaceablesManager.Instance.DropNewFoodPieceIntoTank();
        else StatusLog.Instance.DebugLog("Please move your device directly above the tank and shake to drop food");
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
            if (hit.collider.CompareTag(FishTank.WaterTag))
            {
                tank = hit.collider.GetComponentInParent<FishTank>();
                return tank;
            }
        }
        return null;
    }

    private void TryTogglePlaceableObjectBounds(GameObject placeable, bool show)
    {
        if (placeable &&
            placeable.TryGetComponent(out FishTank tank))
            tank.ToggleBounds(show);
    }
}