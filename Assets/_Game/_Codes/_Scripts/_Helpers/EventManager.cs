using System;
using UnityEngine;

public static class EventManager
{
    public static class StickmanEvents{

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action<int> MoveStarting;
        public static void Moving(int unitId) => MoveStarting?.Invoke(unitId);

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action<int> OnTargetReached;
        public static void TargetReached(int unitId) => OnTargetReached?.Invoke(unitId);

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action<int> MoveToBus;
        public static void StickmanMoveToBus(int id) => MoveToBus?.Invoke(id);

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action<int> MoveBusQueueGrid;
        public static void StickmanMoveToBusQueueGrid(int id) => MoveBusQueueGrid?.Invoke(id);

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action<Bus,int> StickmanArrivedBus;
        public static void ArrivedBus(Bus _bus,int id) => StickmanArrivedBus?.Invoke(_bus,id);

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action MoveToBusFromWaitingGrid;
        public static void StickmanMoveToBusFromWaitingGrid() => MoveToBusFromWaitingGrid?.Invoke();

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action ControlOutline;
        public static void StickmanControlOutline() => ControlOutline?.Invoke();

    }

    public static class BusEvents{

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action BusQueueGoForward;
        public static void QueueGoForward() => BusQueueGoForward?.Invoke();

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static event Action<int> OnBusArrived;
        public static void BusArrived(int busId) => OnBusArrived?.Invoke(busId);

        //-----------------------------------------------------------------------------------------------------------------------------------

        public static event Action<Bus> OnPassengerLoaded;
        public static void PassengerLoaded(Bus bus) => OnPassengerLoaded?.Invoke(bus);
    }

    public static class Ui
    {
        public static System.Action ShowLevelStarterUI = delegate { };
        public static System.Action<int> UpdateLevelText = delegate { };
        public static System.Action LevelFailed = delegate { };
        public static System.Action LevelCompleted = delegate { };
        public static System.Action HideAllUI = delegate { };
    }
}

