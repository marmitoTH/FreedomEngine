using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Player/Player Camera")]
public class PlayerCamera : MonoBehaviour
{
	[Header("Components")]
	public Player player;
	public Camera mainCamera;
	public Camera skyboxCamera;
	public Transform scroller;

	[Header("Parameters")]
	public float maxSpeed;
	public float lookDelay;
	public float lookSpeed;
	public float maxLookDistance;

	private float distance;
	private float frustumHeight;
	private float frustumWidth;
	private float lookTimer;

	private Vector3 position;
	private Vector3 lookPosition;

	private void Start()
	{
		distance = transform.position.z;
	}

	private void LateUpdate()
	{
		GetCameraState();
		FollowPlayer();
		HandleLook();
		SetCameraState();
		HandleClamp();
	}

	private void GetCameraState()
	{
		position = transform.position;
		lookPosition = scroller.localPosition;
		frustumHeight = 2.0f * position.z * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
		frustumWidth = frustumHeight * mainCamera.aspect;
	}

	private void SetCameraState()
	{
		transform.position = position;
		scroller.localPosition = lookPosition;
	}

	private void FollowPlayer()
	{
		var deltaTime = Time.deltaTime;
		var targetPosition = player.transform.position + transform.forward * distance;

		if (!player.disableCameraFollow)
		{
			var horizontalOffset = targetPosition.x - position.x;
			var verticalOffset = targetPosition.y - position.y;
			var maxDelta = maxSpeed * deltaTime;

			if (horizontalOffset > 0)
			{
				position.x += Mathf.Min(horizontalOffset, maxDelta);
			}
			else if (horizontalOffset < 0)
			{
				position.x += Mathf.Max(horizontalOffset, -maxDelta);
			}

			if (verticalOffset > 0)
			{
				position.y += Mathf.Min(verticalOffset, maxDelta);
			}
			else if (verticalOffset < 0)
			{
				position.y += Mathf.Max(verticalOffset, -maxDelta);
			}
		}
	}

	private void HandleLook()
	{
		var deltaTime = Time.deltaTime;
		var maxDelta = lookSpeed * deltaTime;

		if ((player.velocity.sqrMagnitude == 0) && (player.lookingDown || player.lookingUp))
		{
			lookTimer -= deltaTime;

			if (lookTimer <= 0)
			{
				lookPosition.y += Mathf.Sign(player.input.vertical) * maxDelta;
			}
			else
			{
				lookPosition.y = Mathf.MoveTowards(lookPosition.y, 0, maxDelta);
			}

			lookPosition.y = Mathf.Clamp(lookPosition.y, -maxLookDistance, maxLookDistance);
		}
		else
		{
			lookTimer = lookDelay;
			lookPosition = Vector3.MoveTowards(lookPosition, Vector3.zero, maxDelta);
		}
	}

	private void HandleClamp()
	{
		transform.position = ClampToStageBounds(transform.position);
		scroller.position = ClampToStageBounds(scroller.position);
	}

	private Vector3 ClampToStageBounds(Vector3 position)
	{
		var stageManager = StageManager.Instance;

		if (stageManager)
		{
			position.x = Mathf.Max(position.x, StageManager.Instance.bounds.xMin - (frustumWidth * 0.5f));
			position.x = Mathf.Min(position.x, StageManager.Instance.bounds.xMax + (frustumWidth * 0.5f));
			position.y = Mathf.Max(position.y, StageManager.Instance.bounds.yMin - (frustumHeight * 0.5f));
			position.y = Mathf.Min(position.y, StageManager.Instance.bounds.yMax + (frustumHeight * 0.5f));
		}

		return position;
	}
}