using UnityEngine;

[System.Serializable]
public class BusConfig{
    public Colors busColor;
    [Range(1, 5)] public int capacity;
}


public enum Colors{
    White,
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Gray,
    Spawner
}
