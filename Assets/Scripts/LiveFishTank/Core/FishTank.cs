using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Events;

public class FishTank : MonoBehaviour, IVisibilityToggleable
{
    #region Consts
    public const string GlassTag = "TankGlass";
    public const string WaterTag = "TankWater";
    #endregion

    public FishTankData SaveData => _saveData;
    public bool Visible => _isVisible;
    [SerializeField] private GameObject _warningTint;
    [HideInInspector] public UnityEvent<FishFood> FoodPieceConsumed;

    private List<Fish> _fishes;
    private List<FishFood> _foodPieces;
    private List<Ornament> _ornaments;
    private FishTankData _saveData;

    private List<TankWall> _tankWalls;
    private DimBoxes.BoundBox _edgeHighlight;
    private int _nextFoodPiece = 0;
    private bool _isVisible = false;

    protected virtual void Awake()
    {
        _fishes = new();
        _foodPieces = new();
        _ornaments = new();
        _edgeHighlight = GetComponent<DimBoxes.BoundBox>();
        _tankWalls = GetComponentsInChildren<TankWall>().ToList();
    }

    /// <summary>
    /// Saves the <c>FishTank</c>'s current data into <c>SaveData</c>
    /// </summary>
    public virtual void Save()
    {
        var residentsDataList = _fishes.Select(f => f.SaveAndReturnData()).ToList();
        residentsDataList.AddRange(_ornaments.Select(o => o.SaveAndReturnData()).ToList());
        residentsDataList.AddRange(_foodPieces.Select(fg => fg.SaveAndReturnData()).ToList());
        _saveData = new(residentsDataList);
    }

    public FishTankData SaveAndReturnData()
    {
        Save();
        return _saveData;
    }

    public virtual void Restore(FishTankData tankData)
    {
        _saveData = tankData;
        foreach (var residentData in _saveData.TankResidentsDataList)
        {
            GameObject residentPrefab = residentData.Type switch
            {
                TankResidentType.Fish => PlaceablesManager.Instance.GetFishPrefabAtIndex(residentData.PrefabIndex),
                TankResidentType.FishFood => PlaceablesManager.Instance.GetFishFoodPrefabAtIndex(residentData.PrefabIndex),
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
        if (_foodPieces.Count == 0)
            return null;

        if (_nextFoodPiece >= _foodPieces.Count)
            _nextFoodPiece = 0;
        while (_nextFoodPiece < _foodPieces.Count)
        {
            if (_foodPieces[_nextFoodPiece].InTank)
                return _foodPieces[_nextFoodPiece];
            _nextFoodPiece++;
        }
        return null;
    }

    public void OnFoodPieceConsumed(FishFood consumedPiece)
    {
        if (_foodPieces.Contains(consumedPiece))
        {
            _foodPieces.Remove(consumedPiece);
            FoodPieceConsumed.Invoke(consumedPiece);
        }
    }

    public void AddTankResident(TankResident resident)
    {
        switch (resident)
        {
            case Fish f:
                if (!_fishes.Contains(f))
                    _fishes.Add(f);
                break;
            case FishFood ff:
                if (!_foodPieces.Contains(ff))
                    _foodPieces.Add(ff);
                break;
            case Ornament o:
                if (!_ornaments.Contains(o))
                    _ornaments.Add(o);
                break;
        }
        if (!_isVisible)
            resident.ToggleVisibility(_isVisible);
    }

    public void DestroyTankResident(TankResident resident)
    {
        switch (resident)
        {
            case Fish f:
                if (_fishes.Contains(f))
                    _fishes.Remove(f);
                break;
            case FishFood ff:
                if (_foodPieces.Contains(ff))
                    _foodPieces.Remove(ff);
                break;
            case Ornament o:
                if (_ornaments.Contains(o))
                    _ornaments.Remove(o);
                break;
        }
        Destroy(resident.gameObject);
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

        foreach (var food in _foodPieces)
            food.ToggleVisibility(visible);

        foreach (var o in _ornaments)
            o.ToggleVisibility(visible);
    }

    public void CheckToggleWarningColor()
    {
        foreach (var wall in _tankWalls)
        {
            if (wall.IsCollidingWithVerticalPlane)
            {
                _warningTint.SetActive(true);
                return;
            }
        }
        _warningTint.SetActive(false);
    }
}
