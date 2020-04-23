using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class EnemyMotor : FreedomObject
{
    [Header("Base Settings")]
    public bool active = true;
    public bool hurtPlayer = true;

    [Header("Base Components")]
    public GameObject model;
    public AudioClip explosionClip;
    public ParticleSystem explosionParticle;

    private new Collider collider;
    private new AudioSource audio;
    private new Camera camera;

    public Vector3 startPosition { get; private set; }
    public Quaternion startRotation { get; private set; }

    public bool isMotorVisible => IsInViewport(transform.position);

    private void Start()
    {
        InitializeCollider();
        InitializeAudio();
        InitializeCamera();
        GetInitialState();
        OnMotorStart();
    }

    private void InitializeCollider()
    {
        if (!TryGetComponent(out collider))
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
    }

    private void Update()
    {
        if (active)
        {
            if (isMotorVisible)
            {
                var deltaTime = Time.deltaTime;

                OnMotorUpdate(deltaTime);
            }
            else if (!IsInViewport(startPosition))
            {
                transform.position = startPosition;
                transform.rotation = startRotation;
                OnMotorRepositioned();
            }
        }
    }

    public override void OnRespawn()
    {
        Disable();
        Enable();
        OnMotorRespawned();
    }

    private void InitializeAudio()
    {
        if (!TryGetComponent(out audio))
        {
            audio = gameObject.AddComponent<AudioSource>();
        }
    }

    private void InitializeCamera()
    {
        camera = Camera.main;
    }

    private void GetInitialState()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void Disable()
    {
        active = false;
        model.SetActive(false);
        collider.enabled = false;
        StopAllCoroutines();
    }

    public void Enable()
    {
        active = true;
        model.SetActive(true);
        collider.enabled = true;
        transform.SetPositionAndRotation(startPosition, startRotation);
    }

    public bool IsInViewport(Vector3 point)
    {
        var viewPortPoint = camera.WorldToViewportPoint(point);
        var insideHorizontal = (viewPortPoint.x >= -0.2f) && (viewPortPoint.x <= 1.2f);
        var insideVertical = (viewPortPoint.y >= -1f) && (viewPortPoint.y <= 2f);
        return insideHorizontal && insideVertical;
    }

    protected void Explode()
    {
        Disable();
        explosionParticle.Play();
        audio.PlayOneShot(explosionClip);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            if (!player.attacking && hurtPlayer)
            {
                player.ApplyHurt(transform.position);
            }
            else
            {
                if (player.position.y > transform.position.y)
                {
                    player.velocity.y *= -1;
                }

                Explode();
            }
        }
    }

    protected virtual void OnMotorStart() { }

    protected virtual void OnMotorUpdate(float deltaTime) { }

    protected virtual void OnMotorRespawned() { }

    protected virtual void OnMotorRepositioned() { }
}
