using UnityEngine;

public class FishTank : GeospatialObject
{
    [HideInInspector] public string Name;
    public Vector3 TankSize => transform.localScale;

    public void SetTankWidth(float width)
    {
        transform.localScale = new Vector3(width, transform.localScale.y, transform.localScale.z);
    }

    public void SetTankLength(float length)
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, length);
    }

    public void SetTankHeight(float height)
    {
        transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
    }

    public override void Innit(int prefabIndex = 0)
    {
        base.Innit();
        Name = $"Fish tank {PlaceablesManager.Instance.FishTanksCount + 1}";
    }

    public override void Save()
    {
        base.Save();
        SaveData.OtherData = JsonUtility.ToJson(
            new FishTankData
            {
                Name = Name,
                TankSize = new Vector3(TankSize.x, TankSize.y, TankSize.z),
            });
    }

    public override void Restore(GeospatialObjectData geoData)
    {
        base.Restore(geoData);
        var tankData = JsonUtility.FromJson<FishTankData>(geoData.OtherData);
        Name = tankData.Name;
        transform.localScale = tankData.TankSize;
    }
}

public class FishTankData
{
    public string Name;
    public Vector3 TankSize;
}