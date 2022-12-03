using UnityEngine;

public abstract class PlaceableObject : MonoBehaviour
{
    public abstract void Innit(int prefabIndex = 0);
}

public enum PlaceableType
{
    Tank,
    Fish,
    Ornament
}