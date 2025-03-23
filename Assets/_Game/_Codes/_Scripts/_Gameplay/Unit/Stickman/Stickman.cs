using System.Collections;
using DG.Tweening;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Stickman : Unit
{
    private SkinnedMeshRenderer stickmanRenderer;

    private StickmanState currentState;
    public bool IsMoving { get; private set; } = false;
    public bool OnWaitingGrid { get; private set; } = false;
    public bool MyWaitingGridIsLast { get; private set; } = false;

    public WaitingGrid WaitingGrid { get; private set; }

    private CharacterAnimatorController AnimatorController;

    #region editorden aktif state gormek icin
    [SerializeField] private string debugState;
    public string CurrentStateName => currentState?.GetType().Name ?? "Idle";
    #endregion

    private void Awake(){
        AnimatorController = GetComponent<CharacterAnimatorController>();
        if (stickmanRenderer == null){
            stickmanRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }
    }
    private void Update() {
        debugState = CurrentStateName; //Aktif State gormek icin,silinebilir.
    }
    private void OnEnable(){
        EventManager.StickmanEvents.MoveStarting += SetMoving;
        EventManager.StickmanEvents.OnTargetReached += MoveForDecision;
        EventManager.StickmanEvents.MoveToBus += MoveToBus;
        EventManager.StickmanEvents.MoveBusQueueGrid += MoveToBusQueue;
        EventManager.StickmanEvents.StickmanArrivedBus += SitBus;
        EventManager.StickmanEvents.MoveToBusFromWaitingGrid += MoveToBusFromWaitingGrid;
        EventManager.BusEvents.OnBusArrived += InterruptForMoveToWaitingGrid;

    }
    private void OnDisable(){
        DOTween.Kill(gameObject);
        EventManager.StickmanEvents.MoveStarting -= SetMoving;
        EventManager.StickmanEvents.OnTargetReached -= MoveForDecision;
        EventManager.StickmanEvents.MoveToBus -= MoveToBus;
        EventManager.StickmanEvents.MoveBusQueueGrid -= MoveToBusQueue;
        EventManager.StickmanEvents.StickmanArrivedBus -= SitBus;
        EventManager.StickmanEvents.MoveToBusFromWaitingGrid -= MoveToBusFromWaitingGrid;
        EventManager.BusEvents.OnBusArrived -= InterruptForMoveToWaitingGrid;
        StopAllCoroutines();
    }
    private void OnTriggerEnter(Collider other){
        if (other.CompareTag(Consts.Tags.DECISIONWALL)){
            Colors activeBusColor = BusQueueManager.Instance.GetActiveBusColor();

            if (activeBusColor == UnitColor){
                EventManager.StickmanEvents.StickmanMoveToBus(UnitID);
            }
            else{
                EventManager.StickmanEvents.StickmanMoveToBusQueueGrid(UnitID);
            }
        }
        if (other.TryGetComponent<Bus>(out Bus _bus) && _bus.UnitColor == UnitColor){
            StopAllCoroutines();
            EventManager.StickmanEvents.ArrivedBus(_bus, UnitID);
        }
    }
    public override void Initialize(Colors color, int capacity = 1){
        UnitColor = color;
        Capacity = capacity;
        levelEditorDatas = GameManager.Instance.GetLevelEditorDatas();

        stickmanRenderer ??= GetComponentInChildren<SkinnedMeshRenderer>();
        if (stickmanRenderer == null){
            Debug.LogError("Stickman SkinnedMeshRenderer bulunamadı!");
            return;
        }
        ApplyMaterial();
    }
    protected override void ApplyMaterial(){
        if (stickmanRenderer == null){
            Debug.LogError("Stickman SkinnedMeshRenderer atanmadı!");
            return;
        }
        Material material = MaterialHelper.GetMaterialFromColor(UnitColor, levelEditorDatas);
        if (material == null){
            Debug.LogWarning($"Materyal bulunamadı: {UnitColor}");
            return;
        }
        stickmanRenderer.material = material;
    }
    public void ChangeState(StickmanState newState){
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();
    }
    private void MoveForDecision(int unitId){
        if (UnitID == unitId){
            IsMoving = false;
            ChangeState(new MoveToDecisionState(this));
        }
    }
    private void MoveToBus(int unitId){
        if (UnitID == unitId){
            ChangeState(new MoveToBusState(this));
        }
    }
    private void MoveToBusFromWaitingGrid(){
        if (BusQueueManager.Instance.GetActiveBusColor() == UnitColor){
            if (IsMoving){
                ChangeState(new MoveToBusState(this));
            }

            if (OnWaitingGrid){
                ChangeState(new MoveToBusState(this));
                WaitingGrid.IsOccupied = false;
            }
        }       
    }
    private void MoveToBusQueue(int unitId){
        if (UnitID == unitId){
            ChangeState(new MoveToQueueState(this));
        }
    }
    private void SitBus(Bus _bus, int id){
        if (id != UnitID) return;
        ChangeState(new SitBusState(this, _bus));
    }
    private void InterruptForMoveToWaitingGrid(int busId = 0){
        //StopAllCoroutines();        
        if (BusQueueManager.Instance.GetActiveBusColor() == UnitColor){

            StickmanAPathfinding stickmanAPathfinding = GetComponent<StickmanAPathfinding>();
            if (stickmanAPathfinding.GetMoveCoroutine() != null)
            {
                Debug.LogError("MoveToWaitingGrid durduruldu");
                stickmanAPathfinding.StopMoveCourutine();
            }

            ChangeState(new MoveToBusState(this));
        }
    }
    public void SetMoving(int id){
        if (UnitID == id){
            IsMoving = true;
        }
    }
    private Transform currentTarget;
    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }
    public IEnumerator MoveToTarget(Transform target, float speed, bool rotating = false){
        if (target == null){
            Debug.LogWarning("MoveToTarget: Hedef null! Coroutine durduruluyor.");
            yield break;
        }

        float moveTime = 0f;
        float maxMoveTime = 5f; // Maksimum hareket süresi (5 saniye)

        if (!rotating){
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        EventManager.StickmanEvents.Moving(UnitID);

        while (target != null &&
            transform != null && 
            Vector3.Distance(transform.position, target.position) > 0.1f){
            if (currentTarget.position != target.position)
            {
                yield break;
            }
            if (rotating){
                Vector3 direction = (target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 9f);
            }

            transform.position += transform.forward * (speed * Time.deltaTime);
            moveTime += Time.deltaTime;

            if (moveTime >= maxMoveTime){
                Debug.LogError($"Stickman {UnitID} hedefe ulaşamıyor! Pozisyon sıfırlanıyor.");
                transform.position = target.position; // Takıldıysa doğrudan hedefe taşı
                break;
            }

            yield return null;
        }
        if (transform != null)
        {
            transform.DORotateQuaternion(Quaternion.Euler(Vector3.zero), 0.5f).SetEase(Ease.OutQuad);
        }
        
        IsMoving = false;
        AnimatorController.ChangeAnimationState(AnimationStates.Idle);
    }
    public void SetOnWaitingGridBool(bool wait){
        OnWaitingGrid = wait;
    }
    public void SetOnWaitingGrid(WaitingGrid waitingGrid){
        WaitingGrid = waitingGrid;
        waitingGrid.IsOccupied = true;
        MyWaitingGridIsLast = BusWaitingGridManager.Instance.AreAllGridsFull();
    }
    public void StopAllCourutine(){
        StopAllCoroutines();
    }
}
