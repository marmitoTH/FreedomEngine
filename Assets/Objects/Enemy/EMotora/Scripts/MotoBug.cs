using UnityEngine;
using System.Collections;

[AddComponentMenu("Freedom Engine/Objects/Enemy/MotoBug")]
public class MotoBug : EnemyMotor
{
    [Header("MotoBug Settings")]
    public float speed;
    public float rotateTime;
    public float groundDistance;
    public float groundRayDistance;
    public float wallRayDistance;
    public LayerMask solidLayer;

    //[Header("MotoBug Wheel")]
    //public float wheelRotationAngle;
    //public Transform wheel;

    private float direction;
    private bool turning;

    protected override void OnMotorStart()
    {
        InitializeMotoBug();
    }

    protected override void OnMotorUpdate(float deltaTime)
    {
        if (!turning)
        {
            var position = transform.position;
            position += Vector3.right * direction * speed * deltaTime;

            if (Physics.Raycast(position, Vector3.right * direction, wallRayDistance, solidLayer))
            {
                StartCoroutine(Turn());
            }

            if (Physics.Raycast(position, Vector3.down, out var ground, groundRayDistance, solidLayer))
            {
                position.y = ground.point.y + groundDistance;
            }
            else
            {
                StartCoroutine(Turn());
            }

            //wheel.transform.Rotate(wheelRotationAngle * deltaTime, 0, 0);
            transform.position = position;
        }
    }

    protected override void OnMotorRespawned()
    {
        InitializeMotoBug();
    }

    protected override void OnMotorRepositioned()
    {
        InitializeMotoBug();
    }

    private void InitializeMotoBug()
    {
        turning = false;
        direction = -1;
    }

    private IEnumerator Turn()
    {
        var elapsedTime = 0f;
        var oldEulerY = transform.eulerAngles.y;
        
        turning = true;

        while (elapsedTime < rotateTime)
        {
            var alpha = elapsedTime / rotateTime;
            var newEulerY = Mathf.LerpAngle(oldEulerY, 90 * direction - 90, alpha);

            transform.rotation = Quaternion.Euler(0, newEulerY, 0);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        direction *= -1;
        turning = false;
    }
}
