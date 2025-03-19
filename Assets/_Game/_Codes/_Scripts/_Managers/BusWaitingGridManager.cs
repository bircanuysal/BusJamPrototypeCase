using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BusWaitingGridManager : LocalSingleton<BusWaitingGridManager>
{
    private List<WaitingGrid> waitingGrids;

    protected override void Awake()
    {
        base.Awake();
        waitingGrids = GetComponentsInChildren<WaitingGrid>().ToList();
    }

    public WaitingGrid GetFirstAvailableGrid()
    {
        WaitingGrid firstGrid = waitingGrids
            .FirstOrDefault(grid => !grid.IsOccupied); // Ýlk boþ olaný bul

        return firstGrid != null ? firstGrid : null;
    }
    public bool AreAllGridsFull()
    {
        return waitingGrids.All(grid => grid.IsOccupied);
    }
    public void OccupyGrid(WaitingGrid gridTransform , bool isEmpty)
    {
        if (gridTransform != null)
        {
            gridTransform.IsOccupied = isEmpty;
        }
    }
}
