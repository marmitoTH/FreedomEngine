using UnityEngine;

public abstract class PlayerMotor : MonoBehaviour
{
	[Header("Base Settings")]
	public bool simulate = true;
	public bool lockUpright;

	[Header("Collider Settings")]
	public float height;
	public float wallExtents;
	public float groundExtents;
	public Bounds[] bounds;

	[Header("Collision Masks")]
	public LayerMask groundLayer;
	public LayerMask wallLayer;
	public LayerMask ceilingLayer;

	[Space(10)]
	public Vector2 velocity;

	public Bounds currentBounds { get; private set; }
	public bool grounded { get; private set; }

	private new BoxCollider collider;
	private new Rigidbody rigidbody;

	private const float maxTimeStep = 1 / 60f;

	public Vector3 position { get; protected set; }
	public Vector3 right { get; private set; }
	public Vector3 up { get; private set; }
	public bool disableCollision { get; private set; }
	public float angle { get; private set; }

	private void Start()
	{
		StartMotor();
		OnMotorStart();
	}

	private void Update()
	{
		OnMotorUpdate();
		SimulatePhysics();
	}

	private void LateUpdate()
	{
		OnMotorLateUpdate();
	}

	private void StartMotor()
	{
		InitializeRigidbody();
		InitializeCollider();
	}

	private void InitializeRigidbody()
	{
		if (!TryGetComponent(out rigidbody))
		{
			rigidbody = gameObject.AddComponent<Rigidbody>();
		}

		rigidbody.isKinematic = true;
	}

	private void InitializeCollider()
	{
		if (!TryGetComponent(out collider))
		{
			collider = gameObject.AddComponent<BoxCollider>();
		}

		collider.isTrigger = true;
		ChangeBounds(0);
	}

	private void GetPhysicsState()
	{
		position = transform.position;
		right = transform.right;
		up = transform.up;
		angle = Vector3.Angle(up, Vector3.up);
	}

	private void SetPhysicsState()
	{
		transform.position = position;
		transform.LookAt(transform.position + transform.forward, up);
	}

	private void SimulatePhysics()
	{
		if (simulate)
		{
			GetPhysicsState();

			var frameTime = Time.deltaTime;

			while (frameTime > 0f)
			{
				var deltaTime = Mathf.Min(frameTime, maxTimeStep);

				OnMotorFixedUpdate(deltaTime);
				MotorFixedUpdate(deltaTime);

				frameTime -= deltaTime;
			}

			SetPhysicsState();
		}
	}

	private void MotorFixedUpdate(float deltaTime)
	{
		UpdateGroundState();
		UpdateCollision(deltaTime);
	}

	private void UpdateGroundState()
	{
		if (grounded && velocity.y > 0)
		{
			GroundExit();
		}
	}

	private void UpdateCollision(float deltaTime)
	{
		if (!disableCollision)
		{
			var horizontalTranslation = right * velocity.x * deltaTime;
			var verticalTranslation = up * velocity.y * deltaTime;

			HorizontalCollision(horizontalTranslation.normalized, horizontalTranslation.magnitude);
			VerticalCollision(verticalTranslation.normalized, verticalTranslation.magnitude);
		}
		else
		{
			position += (Vector3)velocity * deltaTime;
		}
	}

	private void HorizontalCollision(Vector3 direction, float distance)
	{
		var origin = position + up * currentBounds.center.y;
		var offset = currentBounds.extents.x + wallExtents;
		var destination = position + direction * distance;

		if (Physics.Raycast(origin, direction, out var hit, distance + offset, wallLayer))
		{
			CallContact(hit.collider);

			if (Vector3.Dot(transform.InverseTransformVector(velocity), hit.normal) <= 0 && hit.collider.enabled)
			{
				var safeDistance = hit.distance - offset;
				destination = position + direction * safeDistance;
				velocity.x = 0;
			}
		}

		position = destination;
	}

	private void VerticalCollision(Vector3 direction, float distance)
	{
		var offset = height * 0.5f;
		var destination = position + direction * distance;
		var layer = (direction.y <= 0) ? groundLayer : ceilingLayer;

		if (!grounded)
		{
			if (GroundRaycast(direction, distance, offset, out var groundInfo, out _, layer))
			{
				var movingTowardsGround = (Vector3.Dot(velocity, groundInfo.normal) <= 0);
				var validSurface = (Vector2.Angle(groundInfo.normal, Vector2.up) < 135f);

				if (movingTowardsGround)
				{
					var safeDistance = groundInfo.distance - offset;

					if (validSurface)
					{
						GroundEnter(groundInfo.normal);

						if (groundInfo.collider.CompareTag("MovingPlatform"))
						{
							transform.parent = groundInfo.collider.transform;
						}
					}

					velocity.y = 0;
					destination = position + direction * safeDistance;
				}
			}
		}
		else
		{
			var groundRaySize = offset + groundExtents;
			var colliding = GroundRaycast(-up, 0, groundRaySize, out var groundInfo, out var snap, layer);

			if (colliding && velocity.y <= 0)
			{
				if (snap)
				{
					up = groundInfo.normal;
					var safeDistance = groundInfo.distance - offset;
					destination = position - up * safeDistance;
				}
			}
			else
			{
				GroundExit();
			}
		}

		position = destination;
	}

	private bool GroundRaycast(Vector3 direction, float distance, float offset, out FreedomCollision hit, out bool snap, LayerMask layer)
	{
		var hitDistance = 0f;
		var hitPoint = Vector3.zero;
		var hitNormal = Vector3.zero;
		var raySize = distance + offset;
		var leftRayOrigin = position - right * currentBounds.extents.x;
		var rightRayOrigin = position + right * currentBounds.extents.x;
		var leftRay = Physics.Raycast(leftRayOrigin, direction, out var leftHit, raySize, layer);
		var rightRay = Physics.Raycast(rightRayOrigin, direction, out var rightHit, raySize, layer);
		var colliding = snap = false;

		Collider closestCollider = null;

		if (leftRay || rightRay)
		{
			if (leftRay && rightRay)
			{
				if (Vector3.Dot(leftHit.normal, rightHit.normal) > 0.8f)
				{
					snap = true;
					hitPoint = (leftHit.point + rightHit.point) * 0.5f;
					hitNormal = (leftHit.normal + rightHit.normal) * 0.5f;
					hitDistance = (leftHit.distance < rightHit.distance) ? leftHit.distance : rightHit.distance;
					closestCollider = (leftHit.distance < rightHit.distance) ? leftHit.collider : rightHit.collider;
				}
				else
				{
					var closestHit = (leftHit.distance < rightHit.distance) ? leftHit : rightHit;
					hitPoint = closestHit.point;
					hitNormal = closestHit.normal;
					hitDistance = closestHit.distance;
					closestCollider = closestHit.collider;
				}

				CallContact(leftHit.collider);
				CallContact(rightHit.collider);
				colliding = (leftHit.collider.enabled || rightHit.collider.enabled);
			}
			else
			{
				var closestHit = leftRay ? leftHit : rightHit;
				hitPoint = closestHit.point;
				hitNormal = closestHit.normal;
				hitDistance = closestHit.distance;
				CallContact(closestHit.collider);
				closestCollider = closestHit.collider;
				colliding = closestHit.collider.enabled;
			}
		}

		if ((closestCollider != null) && snap)
		{
			if (closestCollider.CompareTag("MovingPlatform"))
			{
				snap = false;
			}
		}

		hit = new FreedomCollision(hitPoint, hitNormal, hitDistance, closestCollider);
		return colliding;
	}

	private void CallContact(Collider collider)
	{
		if (collider.TryGetComponent(out FreedomObject listener))
		{
			listener.OnPlayerMotorContact(this);
		}
	}

	public void EnableCollision(bool value = true)
	{
		disableCollision = !value;
		collider.enabled = value;
	}

	public void ChangeBounds(int index)
	{
		if ((index >= 0) && (index < bounds.Length))
		{
			currentBounds = bounds[index];
			UpdateCollider();
		}
	}
	
	private void UpdateCollider()
	{
		collider.center = currentBounds.center;
		collider.size = currentBounds.size;
	}

	public void GroundEnter(Vector3 normal)
	{
		if (!grounded)
		{
			OnGroundEnter();
			up = normal;
			velocity = AirToGround(velocity, up);
			grounded = true;
		}
	}

	public void GroundExit()
	{
		if (grounded)
		{
			transform.parent = null;
			velocity = GroundToAir(velocity);
			up = Vector3.up;
			grounded = false;
		}
	}

	private Vector2 AirToGround(Vector2 velocity, Vector2 normal)
	{
		return new Vector2(velocity.x * normal.y - velocity.y * normal.x, 0);
	}

	private Vector2 GroundToAir(Vector2 velocity)
	{
		return new Vector2(velocity.x * up.y + velocity.y * up.x, velocity.y * up.y - velocity.x * up.x);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			if (bounds.Length > 0)
			{
				foreach (Bounds b in bounds)
				{
					var offset = transform.position + b.center;

					Gizmos.color = new Color(0, 0, 1, 0.7f);
					Gizmos.DrawWireCube(offset, b.size);
					Gizmos.color = new Color(0, 0, 1, 0.35f);
					Gizmos.DrawCube(offset, b.size);
				}
			}
		}
	}
#endif

	protected virtual void OnMotorStart() { }

	protected virtual void OnMotorUpdate() { }

	protected virtual void OnMotorLateUpdate() { }

	protected virtual void OnMotorFixedUpdate(float deltaTime) { }

	protected virtual void OnGroundEnter() { }
}
