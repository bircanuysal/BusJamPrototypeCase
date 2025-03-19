using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class BusQueueManager : LocalSingleton<BusQueueManager>
{
    private Colors activeBusColor;
    private List<Bus> buses = new List<Bus>();
    private Dictionary<Bus, int> busStickmanCount = new Dictionary<Bus, int>();
    [SerializeField] private GameObject wayPrefab;
    private void Start()
    {
        if (buses.Count > 0)
        {
            DOVirtual.DelayedCall(.5f, () => { activeBusColor = buses.First().UnitColor; });
            buses.First().SetState(BusState.CollectingPassengers);
        }
        GameObject way = Instantiate(wayPrefab, Vector3.zero, Quaternion.identity);
        way.transform.SetParent(transform);
        way.transform.localPosition = Vector3.zero;
    }

    private void OnEnable()
    {
        EventManager.StickmanEvents.StickmanArrivedBus += UploadStickman;
    }

    private void OnDisable()
    {
        EventManager.StickmanEvents.StickmanArrivedBus -= UploadStickman;
    }

    public Colors GetActiveBusColor() => activeBusColor;

    public void SetActiveBusColor(Colors color) => activeBusColor = color;

    public void AddBus(Bus bus)
    {
        buses.Add(bus);
        busStickmanCount[bus] = 0;
        bus.SetState(BusState.WaitingInQueue);

        if (buses.Count == 1)
        {
            SetActiveBusColor(bus.UnitColor);
            bus.SetState(BusState.CollectingPassengers);
        }
    }

    public void UploadStickman(Bus bus, int id = 0)
    {
        if (!busStickmanCount.ContainsKey(bus)) return;

        busStickmanCount[bus]++;

        if (busStickmanCount[bus] >= bus.Capacity)
        {        
            QueueGoForward(bus);
        }
    }

    private void QueueGoForward(Bus bus)
    {
        if (buses.Count == 0 || buses[0] != bus) return;
        
        EventManager.BusEvents.PassengerLoaded(bus);
        buses.RemoveAt(0);
        busStickmanCount.Remove(bus);

        if (buses.Count == 0)
        {
            GameManager.Instance.SetGameMode(GameMode.Success);
            return;
        }

        EventManager.BusEvents.QueueGoForward();

        //if (buses.Count > 0)
        //{
        //    buses[0].GetComponent<Bus>().SetCollectingBusBool(true);
        //}
    }
    public Transform GetActiveBusTransform() => buses.Count > 0 ? buses.First().transform : null;

    public Bus GetActiveBus() => buses.FirstOrDefault();

}
