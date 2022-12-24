using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFood : TankResident
{
    public int GrowthPoints => _growthPoints;
    public bool InTank { get; set; } = false;

    [SerializeField] private int _growthPoints;
    private Rigidbody _rb;
    private float _dragAmount;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _dragAmount = Random.Range(20f, 25f);
    }

    public override void Save()
    {
        base.Save();
        _saveData.Type = TankResidentType.FishFood;
    }

    public override void ToggleVisibility(bool visible)
    {
        GetComponent<Renderer>().enabled = visible;
    }

    public void OnConsumed()
    {
        _tank.OnFoodPieceConsumed(this);
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision col)
    {
        _rb.drag += 15 * Time.fixedDeltaTime;
    }

    void OnTriggerStay(Collider other)
    {
        if (!InTank)
            InTank = true;
        if (other.CompareTag(FishTank.WaterTag))
            _rb.drag = _dragAmount;
    }
}
