using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BusChair : MonoBehaviour
{
    private bool sitStickman = false;
    public bool SitStickman
    {
        get => sitStickman;
        set => sitStickman = value;
    }


    [SerializeField]
    private Transform sitTransform;
    public Transform SitTransform
    {
        get => sitTransform;
        set => sitTransform = value;
    }
}
