using UnityEngine;

public abstract class Unit : MonoBehaviour, IUnit
{
    public int UnitID { get; private set; }
    public Colors UnitColor { get; protected set; }
    public int Capacity { get; protected set; }

    [SerializeField] protected LevelEditorDatas levelEditorDatas;

    private void Start()
    {
        GameManager.Instance.RegisterUnit(this);
    }

    private void OnDestroy()
    {
        //GameManager.Instance.UnregisterUnit(this);
    }

    public void SetUnitID(int id) => UnitID = id;
    public abstract void Initialize(Colors color, int capacity = 0);
    protected abstract void ApplyMaterial();
}

public interface IUnit
{
    int UnitID { get; }
    Colors UnitColor { get; }
    int Capacity { get; }
    void SetUnitID(int id);
    void Initialize(Colors color, int capacity = 0);
}