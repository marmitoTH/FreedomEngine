using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Freedom Engine/Objects/Player/Player")]
[RequireComponent(typeof(PlayerSkin))]
public class Player : PlayerMotor
{
	[Header("Settings")]
	public bool disableInput;
	public bool disableSkinRotation;
	public bool disableCameraFollow;

	[Header("Components")]
	public PlayerInput input;
	public PlayerParticles particles;
	public new PlayerCamera camera;
	public PlayerSkin skin;
	public GameObject lostRing;

	[Header("Scriptables")]
	public PlayerStats stats;
	public PlayerAudio audios;

	private int horizontalSpeedHash;
	private int animationSpeedHash;
	private int stateHash;
	private int groundedHash;

	public PlayerStateMachine state;
	private new AudioSource audio;

	private PlayerShields shield;

	public bool attacking { get; set; }
	public bool lookingDown { get; set; }
	public bool lookingUp { get; set; }
	public bool halfGravity { get; set; }
	public bool invincible { get; set; }

	public float invincibleTimer { get; set; }
	public int direction { get; private set; }

	private readonly Queue<Ring> lostRingsPool = new Queue<Ring>();

	protected override void OnMotorStart()
	{
		InitializeStateMachine();
		InitializeCamera();
		InitializeAudio();
		InitializeSkin();
		InitializeAnimatorHash();
		InitializeLostRingPool();
	}

	protected override void OnMotorFixedUpdate(float deltaTime)
	{
		if (!disableInput)
		{
			input.InputUpdate();
			input.UnlockHorizontalControl(deltaTime);
		}

		UpdateInvincibility(deltaTime);
		state.UpdateState(deltaTime);
		ClampVelocity();
		ClampToStageBounds();
	}

	protected override void OnMotorLateUpdate()
	{
		UpdateSkinTransform();
		UpdateSkinAnimaiton();
	}

	protected override void OnGroundEnter()
	{
		particles.landSmoke.Play();
	}

	private void InitializeStateMachine()
	{
		state = new PlayerStateMachine(this);

		foreach (PlayerState state in GetComponents<PlayerState>())
		{
			this.state.AddState(state);
		}

		state.ChangeState<WalkPlayerState>();
	}

	private void InitializeCamera()
	{
		if (camera != null)
		{
			camera.player = this;
			camera.transform.parent = null;
		}
	}

	private void InitializeAudio()
	{
		if (!TryGetComponent(out audio))
		{
			audio = gameObject.AddComponent<AudioSource>();
		}
	}

	private void InitializeSkin()
	{
		direction = 1;
		skin.root.parent = null;
		skin.ChangeMouth(PlayerSkin.Mouth.Left);
	}

	private void InitializeAnimatorHash()
	{
		horizontalSpeedHash = Animator.StringToHash("HorizontalSpeed");
		animationSpeedHash = Animator.StringToHash("AnimationSpeed");
		groundedHash = Animator.StringToHash("Grounded");
		stateHash = Animator.StringToHash("State");
	}

	private void ClampVelocity()
	{
		velocity = Vector3.ClampMagnitude(velocity, stats.maxSpeed);
	}

	private void ClampToStageBounds()
	{
		var stageManager = StageManager.Instance;

		if (stageManager && !disableCollision)
		{
			var nextPosition = position;
			
			if ((nextPosition.x - currentBounds.extents.x - wallExtents) < stageManager.bounds.xMin)
			{
				var safeDistance = stageManager.bounds.xMin + currentBounds.extents.x;
				nextPosition.x = Mathf.Max(nextPosition.x, safeDistance);
				velocity.x = Mathf.Max(velocity.x, 0);
			}
			else if ((nextPosition.x + currentBounds.extents.x + wallExtents) > stageManager.bounds.xMax)
			{
				var safeDistance = stageManager.bounds.xMax - currentBounds.extents.x;
				nextPosition.x = Mathf.Min(nextPosition.x, safeDistance);
				velocity.x = Mathf.Min(velocity.x, 0);
			}

			if ((nextPosition.y - height * 0.5f) < stageManager.bounds.yMin)
			{
				var safeDistance = stageManager.bounds.yMin - height * 0.5f;
				nextPosition.y = Mathf.Max(nextPosition.y, safeDistance);
				ApplyDeath();
			}

			position = nextPosition;
		}
	}

	private void InitializeLostRingPool()
	{
		for (int i = 0; i < stats.maxLostRingCount; i++)
		{
			var gameObject = Instantiate(lostRing);

			if (gameObject.TryGetComponent(out Ring ring))
			{
				ring.Disable();
				lostRingsPool.Enqueue(ring);
			}
		}
	}

	private Ring InstantiateLostRing(Vector3 position, Quaternion rotation)
	{
		var ring = lostRingsPool.Dequeue();
		ring.transform.SetPositionAndRotation(position, rotation);
		ring.Enable();
		lostRingsPool.Enqueue(ring);
		return ring;
	}

	private void ScatterRings(float amount)
	{
		var angle = 101.25f;
		var flipDirection = false;
		var force = stats.ringScatterForce;
		
		amount = Mathf.Min(amount, stats.maxLostRingCount);

		for (int i = 0; i < amount; i++)
		{
			var ring = InstantiateLostRing(transform.position, Quaternion.identity);
			ring.velocity.y = Mathf.Sin(angle) * force;
			ring.velocity.x = Mathf.Cos(angle) * force;

			if (flipDirection)
			{
				ring.velocity.x *= -1;
				angle += 22.5f;
			}

			flipDirection = !flipDirection;

			if (i == 16)
			{
				force *= 0.5f;
				angle = 101.25f;
			}
		}
	}

	private void UpdateSkinAnimaiton()
	{
		skin.animator.SetFloat(horizontalSpeedHash, Mathf.Abs(velocity.x));
		skin.animator.SetFloat(animationSpeedHash, Mathf.Lerp(0.8f, 3, velocity.magnitude / stats.maxSpeed));
		skin.animator.SetInteger(stateHash, state.stateId);
		skin.animator.SetBool(groundedHash, grounded);
	}

	private void UpdateSkinTransform()
	{
		var yRotation = 90f - direction * 90f;
		var zRotation = (grounded && (angle > stats.minAngleToRotate)) ? transform.eulerAngles.z : 0;
		var newRotation = Quaternion.Euler(0, 0, zRotation) * Quaternion.Euler(0, yRotation, 0);

		if (!disableSkinRotation)
		{
			var maxDegree = 850f * Time.deltaTime;
			skin.root.rotation = Quaternion.RotateTowards(skin.root.rotation, newRotation, maxDegree);
		}

		skin.root.position = position;
	}

	public void UpdateDirection(float direction)
	{
		if (direction != 0)
		{
			this.direction = (direction > 0) ? 1 : -1;
		}
	}

	public void UpdateInvincibility(float deltaTime)
	{
		if (invincible && (invincibleTimer > 0))
		{
			invincibleTimer -= deltaTime;
			
			if (invincibleTimer <= 0)
			{
				invincible = false;
				invincibleTimer = 0;
			}
		}
	}

	public void SetShield(PlayerShields shield)
	{
		this.shield = shield;

		switch (shield)
		{
			case PlayerShields.None:
				particles.normalShield.Stop();
				break;
			case PlayerShields.Normal:
				particles.normalShield.Play();
				break;
		}
	}

	public void PlayAudio(AudioClip clip, float volume = 1f)
	{
		audio.Stop();
		audio.PlayOneShot(clip, volume);
	}

	public void HandleSlopeFactor(float deltaTime)
	{
		if (grounded)
		{
			if (!attacking)
			{
				velocity.x += up.x * stats.slope * deltaTime;
			}
			else
			{
				var downHill = (Mathf.Sign(velocity.x) == Mathf.Sign(up.x));
				var slope = downHill ? stats.slopeRollDown : stats.slopeRollUp;
				velocity.x += up.x * slope * deltaTime;
			}
		}
	}

	public void HandleAcceleration(float deltaTime)
	{
		var acceleration = grounded ? stats.acceleration : stats.airAcceleration;

		if (input.right && (velocity.x < stats.topSpeed))
		{
			velocity.x += acceleration * deltaTime;
			velocity.x = Mathf.Min(velocity.x, stats.topSpeed);
		}
		else if (input.left && (velocity.x > -stats.topSpeed))
		{
			velocity.x -= acceleration * deltaTime;
			velocity.x = Mathf.Max(velocity.x, -stats.topSpeed);
		}
	}

	public void HandleDeceleration(float deltaTime)
	{
		if (grounded)
		{
			var deceleration = attacking ? stats.rollDeceleration : stats.deceleration;

			if (input.right && (velocity.x < 0))
			{
				velocity.x += deceleration * deltaTime;

				if (velocity.x >= 0)
				{
					velocity.x = stats.turnSpeed;
				}
			}
			else if (input.left && (velocity.x > 0))
			{
				velocity.x -= deceleration * deltaTime;

				if (velocity.x <= 0)
				{
					velocity.x = -stats.turnSpeed;
				}
			}
		}
	}

	public void HandleFriction(float deltaTime)
	{
		if (grounded && (attacking || (input.horizontal == 0)))
		{
			var friction = attacking ? stats.rollFriction : stats.friction;
			velocity = Vector3.MoveTowards(velocity, Vector3.zero, friction * deltaTime);
		}
	}

	public void HandleGravity(float deltaTime)
	{
		if (!grounded)
		{
			var gravity = halfGravity ? (stats.gravity * 0.5f) : stats.gravity;
			velocity.y -= gravity * deltaTime;
		}
	}

	public void HandleJump()
	{
		if (grounded)
		{
			PlayAudio(audios.jump, 0.4f);
			velocity.y = stats.maxJumpHeight;
			state.ChangeState<JumpPlayerState>();
		}
	}

	public void HandleFall()
	{
		if (grounded)
		{
			if ((Mathf.Abs(velocity.x) < stats.minSpeedToSlide) && (angle >= stats.minAngleToSlide))
			{
				if (angle >= stats.minAngleToFall)
				{
					GroundExit();
				}

				input.LockHorizontalControl(stats.controlLockTime);
			}
		}
	}
	
	public void ApplyHurt(Vector3 hurtPoint)
	{
		if (!invincible)
		{
			if (shield != PlayerShields.None || ScoreManager.Instance.Rings > 0)
			{
				velocity.y = stats.pushBack;
				velocity.x = stats.pushBack * 0.5f * Mathf.Sign(transform.position.x - hurtPoint.x);
				state.ChangeState<HurtPlayerState>();

				if (shield == PlayerShields.None)
				{
					audio.PlayOneShot(audios.ringLoss, 0.4f);
					ScatterRings(ScoreManager.Instance.Rings);
					ScoreManager.Instance.Rings = 0;
				}
				else
				{
					SetShield(PlayerShields.None);
					audio.PlayOneShot(audios.dead);
				}
			}
			else
			{
				ApplyDeath();
			}
		}
	}

	public void ApplyDeath()
	{
		var scoreManager = ScoreManager.Instance;

		if (scoreManager)
		{
			scoreManager.Die();
		}

		SetShield(PlayerShields.None);
		state.ChangeState<DiePlayerState>();
		skin.ChangeMouth(PlayerSkin.Mouth.Center);
		skin.root.rotation = Quaternion.Euler(0, 90, 0);
	}

	public void Respawn(Vector3 position, Quaternion rotation)
	{
		direction = 1;
		EnableCollision(true);
		velocity = Vector3.zero;
		skin.ChangeMouth(PlayerSkin.Mouth.Left);
		disableSkinRotation = disableCameraFollow = false;
		transform.SetPositionAndRotation(position, rotation);
		state.ChangeState<WalkPlayerState>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("ForwardTrigger"))
		{
			groundLayer |= (1 << 11);
			groundLayer &= ~(1 << 10);
			wallLayer |= (1 << 11);
			wallLayer &= ~(1 << 10);
		}
		else if (other.CompareTag("BackwardTrigger"))
		{
			groundLayer |= (1 << 10);
			groundLayer &= ~(1 << 11);
			wallLayer |= (1 << 10);
			wallLayer &= ~(1 << 11);
		}
	}
}
