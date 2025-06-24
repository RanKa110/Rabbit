using Unity.Cinemachine;
using UnityEngine;

public class BossMovementHandler
{
    private readonly Rigidbody2D _rb;
    private readonly BossController _owner;

    public BossMovementHandler(Rigidbody2D rb, BossController owner)
    {
        _rb = rb;
        _owner = owner;
    }

    public void Chase()
    {
        var target = _owner.Target;

        if (target == null || target.IsDead)
        {
            Debug.LogError("Chase에 이 부분 문제 있음!");
            return;
        }

        Vector2 origin = _rb.position;
        Vector2 goal = target.Collider.bounds.center;
        Vector2 dir = (goal - origin).normalized;

        float speed = _owner.StatManager.GetValue(StatType.MoveSpeed);
        Vector2 nextPos = origin + dir * speed * Time.fixedDeltaTime;

        _rb.MovePosition(nextPos);
    }
}
