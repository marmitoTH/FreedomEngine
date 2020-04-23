using System.Collections;
using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Falling Platform")]
[RequireComponent(typeof(Collider))]
public class FallingPlatform : FreedomObject
{
    public float fallDelay;
    public float gravity;

    private bool falling;
    private Vector3 startPoint;
    private IEnumerator fallCoroutine;

    private new Collider collider;

    private void Start()
    {
        if (!TryGetComponent(out collider))
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }

        startPoint = transform.position;
    }

    private void Update()
    {
        if (falling)
        {
            transform.position += Vector3.down * gravity * Time.deltaTime;

            if (transform.position.y <= StageManager.Instance.bounds.yMin)
            {
                falling = false;
            }
        }
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallDelay);

        falling = true;
        collider.enabled = false;
    }

    public override void OnRespawn()
    {
        if (fallCoroutine != null)
        {
            StopCoroutine(fallCoroutine);
            fallCoroutine = null;
        }

        falling = false;
        collider.enabled = true;
        transform.position = startPoint;
    }

    public override void OnPlayerMotorContact(PlayerMotor motor)
    {
        if (motor.grounded && fallCoroutine == null)
        {
            fallCoroutine = Fall();
            StartCoroutine(fallCoroutine);
        }
    }
}
