using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Ring")]
[RequireComponent(typeof(SphereCollider))]
public class Ring : FreedomObject
{
	[Header("Settings")]
	public bool lost = false;
	public bool collectable = true;
	public LayerMask solidLayer;

	[Header("Movement Parameter")]
	public float gravity;
	public float bounceFactor;
	public float lifeTime;
	public float uncollectibleTime;

	[Header("Components")]
	public Transform model;
	public AudioClip collectSound;
	public ParticleSystem collectParticle;

	private new SphereCollider collider;
	private new AudioSource audio;

	private RaycastHit hit;
	private float uncollectibleTimer;
	private float lifeTimer;

	[HideInInspector]
	public Vector3 velocity;

	private void Start()
	{
		if (!TryGetComponent(out collider))
		{
			collider = gameObject.AddComponent<SphereCollider>();
		}

		if (!TryGetComponent(out audio))
		{
			audio = gameObject.AddComponent<AudioSource>();
		}

		collider.isTrigger = true;
	}

	private void Update()
	{
		if (model.gameObject.activeSelf)
		{
			var deltaTime = Time.smoothDeltaTime;

			HandleCollectibleStatus(deltaTime);
			HandleCollision(deltaTime);
			HandleLifeTime(deltaTime);
		}
	}

	public override void OnRespawn()
	{
		Disable();
		Enable();
	}

	private void HandleCollectibleStatus(float deltaTime)
	{
		if (!collectable)
		{
			uncollectibleTimer += deltaTime;

			if (uncollectibleTimer >= uncollectibleTime)
			{
				collectable = true;
				uncollectibleTimer = 0;
			}
		}
	}

	private void HandleLifeTime(float deltaTime)
	{
		if (lost)
		{
			lifeTimer += deltaTime;

			if (lifeTimer >= lifeTime)
			{
				Disable();
				lifeTimer = 0;
			}
		}
	}

	private void HandleCollision(float deltaTime)
	{
		if (lost)
		{
			velocity.y -= gravity * deltaTime;
			transform.position += velocity * deltaTime;

			if (Physics.Raycast(transform.position, velocity.normalized, out hit, collider.radius, solidLayer))
			{
				velocity = Vector3.Reflect(velocity, hit.normal) * bounceFactor;
				transform.position = hit.point + hit.normal * collider.radius;
			}
		}
	}

	public void Disable()
	{
		collectable = false;
		model.gameObject.SetActive(false);
	}

	public void Enable()
	{
		collectable = !lost;
		lifeTimer = 0;
		uncollectibleTimer = 0;
		model.gameObject.SetActive(true);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (collectable && other.CompareTag("Player"))
		{
			ScoreManager.Instance.Rings++;
			audio.PlayOneShot(collectSound, 0.25f);
			collectParticle.Play();
			Disable();
		}
	}
}
