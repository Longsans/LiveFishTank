using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FishTank : MonoBehaviour, IVisibilityToggleable
{
    public FishTankData SaveData => _saveData;
    public bool Visible => _isVisible;
    [HideInInspector] public BoxCollider Collider;
    [HideInInspector] public UnityEvent<FishFood> FoodPieceConsumed;

    protected List<Fish> _fishes;
    protected List<FishFoodGroup> _fishFoodGroups;
    protected List<Ornament> _ornaments;
    protected FishTankData _saveData;

    private DimBoxes.BoundBox _edgeHighlight;
    private int _nextFoodGroup = 0;
    private bool _isVisible = false;

    protected virtual void Awake()
    {
        _fishes = new();
        _fishFoodGroups = new();
        _ornaments = new();
        Collider = GetComponentInChildren<BoxCollider>();
        _edgeHighlight = GetComponentInChildren<DimBoxes.BoundBox>();
    }

    /// <summary>
    /// Saves the <c>FishTank</c>'s current data into <c>SaveData</c>
    /// </summary>
    public virtual void Save()
    {
        var residentsDataList = _fishes.Select(f => f.SaveAndReturnData()).ToList();
        residentsDataList.AddRange(_ornaments.Select(o => o.SaveAndReturnData()).ToList());
        residentsDataList.AddRange(_fishFoodGroups.Select(fg => fg.SaveAndReturnData()).ToList());
        _saveData = new(
            Collider.gameObject.transform.localScale,
            residentsDataList);
    }

    public FishTankData SaveAndReturnData()
    {
        Save();
        return _saveData;
    }

    public virtual void Restore(FishTankData tankData)
    {
        _saveData = tankData;
        Collider.gameObject.transform.localScale = tankData.Size;

        foreach (var residentData in _saveData.TankResidentsDataList)
        {
            GameObject residentPrefab = residentData.Type switch
            {
                TankResidentType.Fish => PlaceablesManager.Instance.GetFishPrefabAtIndex(residentData.PrefabIndex),
                TankResidentType.FishFood => PlaceablesManager.Instance.GetFishFoodGroupPrefabAtIndex(residentData.PrefabIndex),
                TankResidentType.Ornament => PlaceablesManager.Instance.GetOrnamentPrefabAtIndex(residentData.PrefabIndex),
                _ => PlaceablesManager.Instance.OtherPrefab,
            };

            var residentGameObj = Instantiate(residentPrefab);
            var resident = residentGameObj.GetComponent<TankResident>();

            resident.Restore(residentData, this);
            if (!_isVisible)
                resident.ToggleVisibility(_isVisible);
        }
    }

    public void ToggleBounds(bool show)
    {
        if (InteractionManager.Instance.CurrentSelectedPlaceable != gameObject)
            _edgeHighlight.enabled = show;
    }

    public FishFood GetNextFishFoodInTank()
    {
        if (_fishFoodGroups.Count == 0)
            return null;

        FishFood food;
        if (_nextFoodGroup >= _fishFoodGroups.Count)
            _nextFoodGroup = 0;

        while (!(food = _fishFoodGroups[_nextFoodGroup].GetNextFoodPiece()))
        {
            _nextFoodGroup = ++_nextFoodGroup % _fishFoodGroups.Count;
            if (_nextFoodGroup == 0)
            {
                foreach (var foodGroup in _fishFoodGroups)
                    foodGroup.ResetFoodPiecesIterator();
            }
        }
        return food;
    }

    public void NotifyFoodPieceConsumed(FishFood consumedPiece)
    {
        FoodPieceConsumed.Invoke(consumedPiece);
    }

    public void SetTankWidth(float width)
    {
        Collider.gameObject.transform.localScale = new Vector3(
            width,
            Collider.gameObject.transform.localScale.y,
            Collider.gameObject.transform.localScale.z);
    }

    public void SetTankLength(float length)
    {
        Collider.gameObject.transform.localScale = new Vector3(
            Collider.gameObject.transform.localScale.x,
            Collider.gameObject.transform.localScale.y,
            length);
    }

    public void SetTankHeight(float height)
    {
        Collider.gameObject.transform.localScale = new Vector3(
            Collider.gameObject.transform.localScale.x,
            height,
            Collider.gameObject.transform.localScale.z);
    }


    public void AddTankResident(TankResident resident)
    {
        switch (resident)
        {
            case Fish f:
                if (!_fishes.Contains(f))
                    _fishes.Add(f);
                break;
            case FishFoodGroup fg:
                if (!_fishFoodGroups.Contains(fg))
                    _fishFoodGroups.Add(fg);
                break;
            case Ornament o:
                if (!_ornaments.Contains(o))
                    _ornaments.Add(o);
                break;
        }
        if (!_isVisible)
            resident.ToggleVisibility(_isVisible);
    }

    public void DestroyTankResident(TankResident tankObject)
    {
        switch (tankObject)
        {
            case Fish f:
                if (_fishes.Contains(f))
                    _fishes.Remove(f);
                break;
            case FishFoodGroup fg:
                if (_fishFoodGroups.Contains(fg))
                    _fishFoodGroups.Remove(fg);
                break;
            case Ornament o:
                if (_ornaments.Contains(o))
                    _ornaments.Remove(o);
                break;
        }
        Destroy(tankObject.gameObject);
    }

    public void ToggleVisibility(bool visible)
    {
        InteractionManager.Instance.SelectPlaceableGameObject(null);
        _isVisible = visible;
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = visible;

        foreach (var fish in _fishes)
            fish.ToggleVisibility(visible);

        foreach (var foodGroup in _fishFoodGroups)
            foodGroup.ToggleVisibility(visible);

        foreach (var o in _ornaments)
            o.ToggleVisibility(visible);
    }
}
