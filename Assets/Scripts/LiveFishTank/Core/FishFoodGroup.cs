using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFoodGroup : TankResident
{
    [SerializeField] private List<FishFood> _foodPieces;
    private int _nextPiece = 0;

    public override void Init(int prefabIndex, FishTank tank)
    {
        base.Init(prefabIndex, tank);
        _tank = tank.GetComponent<FishTank>();
        RandomizeFoodPiecesPose();
    }

    public override void Save()
    {
        base.Save();
        _saveData.Type = TankResidentType.FishFood;
    }

    public override void Restore(TankResidentData localData, FishTank tank)
    {
        base.Restore(localData, tank);
        _tank = tank.GetComponent<FishTank>();
        RandomizeFoodPiecesPose();
    }

    public override void ToggleVisibility(bool visible)
    {
        foreach (var foodPiece in _foodPieces)
        {
            foodPiece.GetComponent<Renderer>().enabled = visible;
        }
    }

    public FishFood GetNextFoodPiece()
    {
        if (_nextPiece >= _foodPieces.Count)
            return null;

        var foodPiece = _foodPieces[_nextPiece];
        _nextPiece++;
        return foodPiece;
    }

    public void ResetFoodPiecesIterator()
    {
        _nextPiece = 0;
    }

    public void OnFoodPieceConsumed(FishFood consumedPiece)
    {
        if (_foodPieces.Contains(consumedPiece))
            _foodPieces.Remove(consumedPiece);

        _tank.NotifyFoodPieceConsumed(consumedPiece);
        Destroy(consumedPiece.gameObject);
        if (_foodPieces.Count == 0)
            _tank.DestroyTankResident(this);
    }

    private void RandomizeFoodPiecesPose()
    {
        foreach (var foodPiece in _foodPieces)
        {
            var x = Random.Range(0f, 0.2f);
            var y = Random.Range(0f, 0.2f);
            var z = Random.Range(0f, 0.2f);
            foodPiece.transform.localPosition = new Vector3(x, y, z);
            foodPiece.transform.localRotation = Random.rotation;
        }
    }
}
