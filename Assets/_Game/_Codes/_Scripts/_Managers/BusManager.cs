using System.Collections.Generic;
using UnityEngine;

public class BusManager
{
    private GameObject busPrefab;
    private Transform busParent;

    public BusManager(GameObject busPrefab, Transform busParent)
    {
        this.busPrefab = busPrefab;
        this.busParent = busParent;
    }

    public void SpawnBuses(List<BusConfig> busQueue)
    {
        DestroyChildren(busParent);

        float offsetX = -2.5f;

        for (int i = 0; i < busQueue.Count; i++)
        {
            var busConfig = busQueue[i];

            GameObject busObj = Object.Instantiate(busPrefab, Vector3.zero, Quaternion.identity, busParent);

            busObj.transform.localPosition = new Vector3(offsetX, 0, 0);
            offsetX -= 7.5f;

            busObj.transform.localRotation = Quaternion.Euler(0, 90, 0);

            busObj.name = $"Bus_{busConfig.busColor}";

            Unit unit = busObj.GetComponent<Unit>();
            Bus bus = busObj.GetComponent<Bus>();
            BusQueueManager.Instance.AddBus(bus);
            if (bus != null)
            {
                unit.Initialize(busConfig.busColor, busConfig.capacity);
            }
        }

        Debug.Log("Otobüsler sahneye eklendi!");
    }



    private void DestroyChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }
}
