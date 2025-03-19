using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineRunner
{
    private static readonly Dictionary<float, WaitForSeconds> waitDictionary = new Dictionary<float, WaitForSeconds>();
    private static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private static readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    /// <summary>
    /// Verilen süre için önbelleðe alýnmýþ bir WaitForSeconds nesnesi döner.Zaman kalir ise tüm courutineler buraya aktarilir.
    /// </summary>
    public static WaitForSeconds Wait(float seconds)
    {
        if (!waitDictionary.TryGetValue(seconds, out var wait))
        {
            wait = new WaitForSeconds(seconds);
            waitDictionary[seconds] = wait;
        }
        return wait;
    }

    /// <summary>
    /// WaitForEndOfFrame nesnesini döndürür (Tek instance kullanýr)
    /// </summary>
    public static WaitForEndOfFrame WaitForEndOfFrame()
    {
        return waitForEndOfFrame;
    }

    /// <summary>
    /// WaitForFixedUpdate nesnesini döndürür (Tek instance kullanýr)
    /// </summary>
    public static WaitForFixedUpdate WaitForFixedUpdate()
    {
        return waitForFixedUpdate;
    }
}
