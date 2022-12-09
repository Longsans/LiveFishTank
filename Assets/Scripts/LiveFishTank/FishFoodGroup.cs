using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFoodGroup : LocalObject
{
    [SerializeField] private List<FishFood> _foodPieces;
    private FishTank _tank;

    public override void Init(int prefabIndex, GeospatialObject geoObject)
    {
        base.Init(prefabIndex, geoObject);
        _tank = geoObject.GetComponent<FishTank>();
        RandomizeFoodPiecesPose();
    }

    public override void Save()
    {
        base.Save();
        _saveData.Type = LocalObjectType.FishFood;
    }

    public override void Restore(LocalObjectData localData, GeospatialObject geoObject)
    {
        base.Restore(localData, geoObject);
        _tank = geoObject.GetComponent<FishTank>();
        RandomizeFoodPiecesPose();
    }

    public FishFood GetRandomFoodPiece()
    {
        return _foodPieces[Random.Range(0, _foodPieces.Count - 1)];
    }

    public void OnFoodPieceConsumed(FishFood consumedPiece)
    {
        if (_foodPieces.Contains(consumedPiece))
            _foodPieces.Remove(consumedPiece);

        _tank.NotifyFoodPieceConsumed(consumedPiece);
        Destroy(consumedPiece.gameObject);
        if (_foodPieces.Count == 0)
            _geoObject.DisposeLocalObject(this);
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
