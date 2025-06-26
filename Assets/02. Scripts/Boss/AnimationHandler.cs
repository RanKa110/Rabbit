using UnityEngine;

public class AnimationHandler
{
    private readonly Animator _animator;

    public AnimationHandler(Animator animator)
    {
        _animator = animator;
    }

    public void SetMoveAnimation(bool isMoving)
    {
        _animator.SetBool("isMoving", isMoving);
    }

    public void PlayAttackAnimation()
    {
        _animator?.SetTrigger("Attack");
    }

    public void PlayDeathAnimation()
    {
        _animator?.SetTrigger("Death");
    }

}
