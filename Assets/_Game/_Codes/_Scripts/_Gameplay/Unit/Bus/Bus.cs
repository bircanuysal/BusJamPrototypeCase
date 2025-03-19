using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class Bus : Unit
{
    private Renderer busRenderer;
    private SpriteRenderer spriteRenderer;
    private BusState currentState;
    private Sequence idleSequence;
    [SerializeField] private GameObject sitPrefab;
    [SerializeField] private GameObject sitArea;

    private List<BusChair> sitTransforms = new List<BusChair>();
    public List<BusChair> SitTransforms => sitTransforms;

    //[SerializeField] ParticleSystem smokeParticle;

    private void OnEnable() => SubscribeEvents(true);
    private void OnDisable() => SubscribeEvents(false);

    private void SubscribeEvents(bool subscribe){
        if (subscribe){
            EventManager.BusEvents.BusQueueGoForward += GoForward;
            EventManager.BusEvents.OnBusArrived += OnBusArrived;
            EventManager.BusEvents.OnPassengerLoaded += PassengerOnLoaded;
        }
        else{
            EventManager.BusEvents.BusQueueGoForward -= GoForward;
            EventManager.BusEvents.OnBusArrived -= OnBusArrived;
            EventManager.BusEvents.OnPassengerLoaded -= PassengerOnLoaded;
        }
    }
    public override void Initialize(Colors color, int capacity){
        UnitColor = color;
        Capacity = capacity;
        levelEditorDatas = GameManager.Instance.GetLevelEditorDatas();

        spriteRenderer = GetComponent<SpriteRenderer>();
        busRenderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();

        if (spriteRenderer)
            spriteRenderer.color = ColorPalette.Colors[UnitColor];

        ApplyMaterial();
        GenerateSeats();
        SetState(BusState.WaitingInQueue);
    }
    protected override void ApplyMaterial(){
        if (!busRenderer){
            Debug.LogError("Bus Renderer bulunamadı!");
            return;
        }

        Material material = MaterialHelper.GetMaterialFromColor(UnitColor, levelEditorDatas);
        if (material)
            busRenderer.material = material;
        else
            Debug.LogWarning($"Materyal bulunamadı: {UnitColor}");
    }
    private void GenerateSeats(){
        float posZ = 1f / Capacity;
        float startZ = -((Capacity - 1) * posZ) / 2f;

        for (int i = 0; i < Capacity; i++){
            GameObject sit = Instantiate(sitPrefab, transform);
            sit.transform.SetParent(sitArea.transform);
            sit.transform.localPosition = new Vector3(0f, 0f, startZ + (i * posZ));
            sit.transform.SetParent(transform);
            sitTransforms.Add(sit.GetComponent<BusChair>());
        }
    }
    public Transform GetAvailableSeat(){
        foreach (var chair in SitTransforms){
            if (!chair.SitStickman){
                chair.SitStickman = true;
                return chair.SitTransform;
            }
        }
        return null;
    }
    public void SetState(BusState newState){
        currentState = newState;
        Debug.Log($"Bus {UnitColor} changed state to: {newState}");

        switch (currentState){
            case BusState.WaitingInQueue:
                break;
            case BusState.QueueAdvancing:            
                MoveForward();
                break;
            case BusState.CollectingPassengers:
                //smokeParticle.Play();
                PlayBoardingAnimation();      
                EventManager.StickmanEvents.StickmanMoveToBusFromWaitingGrid();
                break;
            case BusState.CollectedPassengers:
                EventManager.BusEvents.BusQueueGoForward -= GoForward;
                idleSequence?.Kill();
                MoveForward(20f ,true);
                break;
        }
    }
    private void GoForward(){
        SetState(BusState.QueueAdvancing);
    }
    private void OnBusArrived(int id){
        if (id==UnitID){
            BusQueueManager.Instance.SetActiveBusColor(UnitColor);
            SetState(BusState.CollectingPassengers);
        }
    }
    private void PassengerOnLoaded(Bus bus){
        if (this == bus){
            SetState(BusState.CollectedPassengers);
        }
    }
    private void MoveForward(float targetOffset = 7.5f , bool aftedDestroy = false){
        transform.DOMoveX(transform.position.x + targetOffset, .75f).SetEase(Ease.InCubic).OnComplete(() =>{
            Bus activeBus = BusQueueManager.Instance.GetActiveBus();

            if (activeBus != null && activeBus == this){
                EventManager.BusEvents.BusArrived(UnitID);
            }

            if (aftedDestroy){
                Destroy(gameObject);
            }
        });
    }
    private void PlayBoardingAnimation(){
        Quaternion initialRotation = transform.rotation;
        transform.DORotate(new Vector3(5f, 0f, 0f), 0.2f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => transform.DORotate(initialRotation.eulerAngles, 0.2f, RotateMode.FastBeyond360)
            .SetEase(Ease.InQuad)
            .OnComplete(PlayIdleAnimation));
    }
    private void PlayIdleAnimation(){
        idleSequence?.Kill();
        Vector3 initialPosition = transform.position;
        idleSequence = DOTween.Sequence()
            .Append(transform.DOMoveY(initialPosition.y + 0.15f, 0.2f).SetEase(Ease.OutQuad))
            .Append(transform.DOMoveY(initialPosition.y, 0.2f).SetEase(Ease.InQuad))
            .Join(transform.DORotate(new Vector3(0, 0, 3f), 0.15f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine))
            .Append(transform.DORotate(new Vector3(0, 0, -3f), 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine))
            .Append(transform.DORotate(Vector3.zero, 0.15f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine))
            .SetLoops(-1);
    }
}

public enum BusState{
    WaitingInQueue,    // Kuyrukta bekliyor
    QueueAdvancing,    // Kuyruk ilerliyor
    CollectingPassengers, // Yolcu topluyor
    CollectedPassengers   // Yolcu topladı
}
