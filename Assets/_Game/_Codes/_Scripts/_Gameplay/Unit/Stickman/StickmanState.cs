using System.Collections;
using UnityEngine;

public abstract class StickmanState
{
    protected Stickman stickman;

    protected StickmanState(Stickman stickman)
    {
        this.stickman = stickman;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
}
public class IdleState : StickmanState
{
    public IdleState(Stickman stickman) : base(stickman) { }

    public override void EnterState()
    {
        stickman.GetComponent<CharacterAnimatorController>().ChangeAnimationState(AnimationStates.Idle);
    }
}

public class MoveToDecisionState : StickmanState
{
    public MoveToDecisionState(Stickman stickman) : base(stickman) { }

    public override void EnterState()
    {
        stickman.StartCoroutine(Move());        
    }

    private IEnumerator Move()
    {
        Transform decisionWall = GameManager.Instance.GetDecisionWall();
        stickman.SetTarget(decisionWall);
        stickman.GetComponent<CharacterAnimatorController>().ChangeAnimationState(AnimationStates.Run);
        yield return stickman.StartCoroutine(stickman.MoveToTarget(decisionWall, 5f));        
    }

}
public class MoveToBusState : StickmanState
{
    public MoveToBusState(Stickman stickman) : base(stickman) { }

    public override void EnterState()
    {
        //stickman.StopAllCourutine();
        stickman.StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        Transform busTransform = BusQueueManager.Instance.GetActiveBusTransform();
        stickman.SetTarget(busTransform);
        stickman.GetComponent<CharacterAnimatorController>().ChangeAnimationState(AnimationStates.Run);
        yield return stickman.StartCoroutine(stickman.MoveToTarget(busTransform, 5f, true));
        stickman.ChangeState(new IdleState(stickman));
    }
}
public class MoveToQueueState : StickmanState
{
    public MoveToQueueState(Stickman stickman) : base(stickman) { }

    public override void EnterState()
    {
        //stickman.StopAllCourutine();
        stickman.StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        WaitingGrid grid = BusWaitingGridManager.Instance.GetFirstAvailableGrid();
        if (grid != null)
        {
            stickman.SetOnWaitingGridBool(true);
            stickman.SetOnWaitingGrid(grid);
            stickman.SetTarget(grid.transform);
            stickman.GetComponent<CharacterAnimatorController>().ChangeAnimationState(AnimationStates.Run);
            yield return stickman.StartCoroutine(stickman.MoveToTarget(grid.transform, 5f, true));
        }
        else
        {
            Debug.LogError("Grid null");
            GameManager.Instance.SetGameMode(GameMode.Failed);
        }
        stickman.ChangeState(new IdleState(stickman));
    }
}

public class SitBusState : StickmanState
{
    private Bus bus;

    public SitBusState(Stickman stickman, Bus bus) : base(stickman)
    {
        this.bus = bus;
    }

    public override void EnterState()
    {
        stickman.StopAllCourutine();
        Transform sitTransform = bus.GetAvailableSeat();
        if (sitTransform != null)
        {
            stickman.transform.SetParent(bus.transform);
            stickman.transform.position = sitTransform.position;
            stickman.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            stickman.GetComponent<CharacterAnimatorController>().ChangeAnimationState(AnimationStates.Sit);
        }
    }
}