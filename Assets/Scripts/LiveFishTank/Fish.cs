using UnityEngine;

public class Fish : LocalObject
{
    // a multiplier to apply to scale, increases with fish growth
    private float _size;

    // growth period in minutes
    [SerializeField] private int _growthPeriod;

    public override void Innit(int prefabIndex)
    {
        base.Innit(prefabIndex);
        _saveData.Type = LocalObjectType.Fish;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
