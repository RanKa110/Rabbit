using UnityEngine;

public class MovementHandler
{
    private readonly Rigidbody2D _rb;
    private readonly IUnitController _unit;

    public MovementHandler(Rigidbody2D rb, IUnitController unit)
    {
        _rb = rb;
        _unit = unit;
    }

    public void Chase()
    {
        if (_unit.Target == null)
        {
            Debug.LogError("Target 없음");
            return;
        }

        Vector2 dir = (_unit.Target.Collider.bounds.center - _unit.Collider.bounds.center).normalized;
        float speed = _unit.StatManager.GetValue(StatType.MoveSpeed);

        Debug.Log($"이동 방향: {dir}, 속도: {speed}");

        _rb.linearVelocity = new Vector2(dir.x * speed, _rb.linearVelocity.y);
    }

    public void ChaseWithMinDistance(float minDistance)
    {
        if ( _unit.Target == null)
        {
            return;
        }

        float distance = Vector2.Distance(_unit.Target.Collider.bounds.center, _unit.Collider.bounds.center);
        float speed = _unit.StatManager.GetValue(StatType.MoveSpeed);

        Vector2 dir;

        if (distance < minDistance)
        {
            dir = (_unit.Collider.bounds.center - _unit.Target.Collider.bounds.center).normalized;
        }

        else
        {
            dir = (_unit.Target.Collider.bounds.center - _unit.Collider.bounds.center).normalized;
        }

        _rb.linearVelocity = new Vector2(dir.x * speed, _rb.linearVelocity.y);
    }

    public void StopXMovement()
    {
        Debug.Log("정지!");
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }
}
