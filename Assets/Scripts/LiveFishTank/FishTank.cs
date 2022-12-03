using UnityEngine;

public class FishTank : GeospatialObject
{
    [HideInInspector] public string Name;

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

    public override void Innit()
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
                TankSize = Collider.gameObject.transform.localScale,
            });
    }

    public override void Restore(GeospatialObjectData geoData)
    {
        base.Restore(geoData);
        var tankData = JsonUtility.FromJson<FishTankData>(geoData.OtherData);
        Name = tankData.Name;
        Collider.gameObject.transform.localScale = tankData.TankSize;
    }
}

public class FishTankData
{
    public string Name;
    public Vector3 TankSize;
}