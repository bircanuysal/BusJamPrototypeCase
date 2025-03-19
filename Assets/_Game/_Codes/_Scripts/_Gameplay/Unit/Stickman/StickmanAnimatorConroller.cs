using UnityEngine;

public enum AnimationStates
{
    Idle,
    Run,
    Jump,
    Sit
}
public class CharacterAnimatorController : MonoBehaviour
{
    private Animator animator;
    private ICharacterState currentState;

    private IdleAnimState idleState;
    private RunState runState;
    private JumpState jumpState;
    private SitState sitState;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator bile�eni bulunamad�! L�tfen bu script'i bir Animator bile�eni olan GameObject'e ekleyin.");
            return;
        }

        // Stateleri olu�tur
        idleState = new IdleAnimState(this, animator);
        runState = new RunState(this, animator);
        jumpState = new JumpState(this, animator);
        sitState = new SitState(this, animator);

        // Ba�lang�� state'i
        ChangeState(idleState);
    }



    public void ChangeAnimationState(AnimationStates animationStates)
    {
        if (animationStates == AnimationStates.Idle)
        {
            ChangeState(idleState);
        }
        else if (animationStates == AnimationStates.Run)
        {
            ChangeState(runState);
        }
        else if (animationStates == AnimationStates.Jump)
        {
            ChangeState(jumpState);
        }
        else if (animationStates == AnimationStates.Sit)
        {
            ChangeState(sitState);
        }
    }

    public void ChangeState(ICharacterState newState)
    {
        if (newState == null)
        {
            Debug.LogError("ChangeState() fonksiyonuna NULL state g�nderildi!");
            return;
        }

        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }


}
public interface ICharacterState
{
    void Enter();
    void Exit();
}

public class IdleAnimState : ICharacterState
{
    private CharacterAnimatorController character;
    private Animator animator;

    public IdleAnimState(CharacterAnimatorController character, Animator animator)
    {
        this.character = character;
        this.animator = animator;
    }

    public void Enter()
    {
        animator.Play("Idle");
    }

    public void Exit() { }
}
public class RunState : ICharacterState
{
    private CharacterAnimatorController character;
    private Animator animator;

    public RunState(CharacterAnimatorController character, Animator animator)
    {
        this.character = character;
        this.animator = animator;
    }

    public void Enter()
    {
        animator.Play("Run");
    }


    public void Exit() { }
}
public class JumpState : ICharacterState
{
    private CharacterAnimatorController character;
    private Animator animator;

    public JumpState(CharacterAnimatorController character, Animator animator)
    {
        this.character = character;
        this.animator = animator;
    }

    public void Enter()
    {
        animator.Play("Jump");
    }


    public void Exit() { }
}
public class SitState : ICharacterState
{
    private CharacterAnimatorController character;
    private Animator animator;

    public SitState(CharacterAnimatorController character, Animator animator)
    {
        this.character = character;
        this.animator = animator;
    }

    public void Enter()
    {
        animator.Play("Sit");
    }
    public void Exit() { }
}
