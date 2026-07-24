using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator anim;

    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkingHash = Animator.StringToHash("Walking");
    private static readonly int RunningHash = Animator.StringToHash("Running");
    private static readonly int FallingHash = Animator.StringToHash("Falling");
    private static readonly int SlidingHash = Animator.StringToHash("Sliding");

    private AnimState currentState;

    void Start()
    {
        SetAnimState(AnimState.Idle);
    }

    private void ClearMovementStates()
    {
        anim.SetBool(IdleHash, false);
        anim.SetBool(WalkingHash, false);
        anim.SetBool(RunningHash, false);
        anim.SetBool(FallingHash, false);
        anim.SetBool(SlidingHash, false);
    }

    public void SetAnimState(AnimState newState)
    {
        if (currentState == newState)
            return;

        ClearMovementStates();

        switch (newState)
        {
            case AnimState.Idle:
                anim.SetBool(IdleHash, true);
                break;

            case AnimState.Walking:
                anim.SetBool(WalkingHash, true);
                break;

            case AnimState.Running:
                anim.SetBool(RunningHash, true);
                break;

            case AnimState.Falling:
                anim.SetBool(FallingHash, true);
                break;

            case AnimState.Sliding:
                anim.SetBool(SlidingHash, true);
                break;
        }

        currentState = newState;
    }

    public AnimState GetCurrentState()
    {
        return currentState;
    }
}

public enum AnimState
{
    Idle,
    Walking,
    Running,
    Falling,
    Sliding,
    None
}