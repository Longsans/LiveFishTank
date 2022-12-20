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
        var tank = RaycastForFishTankFromScreenPos(touchPosition);
        if (tank)
        {
            switch (ResidentType)
            {
                case TankResidentType.Fish:
                    PlaceablesManager.Instance.DropNewFishIntoTank();
                    break;
                case TankResidentType.FishFood:
                    PlaceablesManager.Instance.DropNewFishFoodGroupIntoTank();
                    break;
            }
        }
        else
            StatusLog.Instance.DebugLog("Please point your device at the fish tank");
    }

    private FishTank RaycastForFishTankFromScreenPos(Vector2 screenPosition)
    {
        FishTank tank;
        Ray ray = _camera.ScreenPointToRay(screenPosition);

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