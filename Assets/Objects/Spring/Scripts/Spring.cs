using UnityEngine;
using System.Collections;

[AddComponentMenu("Freedom Engine/Objects/Spring")]
[RequireComponent(typeof(Collider))]
public class Spring : MonoBehaviour
{
	[Header("General Settings")]
	public float force;
	public Transform mesh;
	public AudioClip springSound;

	[Header("Player Snap Settings")]
	public bool snapPositionX;
	public bool snapPositionY;

	[Header("Control Lock Settings")]
	public bool lockControl;
	public float lockTime;

	[Header("Hide Settings")]
	public bool hidden;
	public float hideDistance;
	public float hideTime;

	private new AudioSource audio;
	private new Collider collider;

	private Vector3 meshStartPosition;
	private Vector3 hidePoint;

	private void Start()
	{
		InitializeCollider();
		InitializeAudioSource();
		InitializeSpring();
	}

	private void InitializeCollider()
	{
		if (!TryGetComponent(out collider))
		{
			collider = gameObject.AddComponent<BoxCollider>();
		}

		collider.isTrigger = true;
	}

	private void InitializeAudioSource()
	{
		if (!TryGetComponent(out audio))
		{
			audio = gameObject.AddComponent<AudioSource>();
		}
	}

	private void InitializeSpring()
	{
		if (hidden)
		{
			meshStartPosition = mesh.position;
			hidePoint = mesh.position - transform.up * hideDistance;
			mesh.position = hidePoint;
		}
	}

	private void HandlePlayerSnaping(Player player)
	{
		if (snapPositionX || snapPositionY)
		{
			var playerPosition = player.transform.position;

			if (snapPositionX)
			{
				playerPosition.x = transform.position.x;
			}

			if (snapPositionY)
			{
				playerPosition.y = transform.position.y;
			}

			player.transform.position = playerPosition;
		}
	}

	private IEnumerator ShowSpring()
	{
		mesh.transform.position = meshStartPosition;

		yield return new WaitForSeconds(2f);

		var elapsedTime = 0f;
		var initialPosition = mesh.position;

		while (elapsedTime < hideTime)
		{
			var alpha = elapsedTime / hideTime;
			mesh.position = Vector3.Lerp(initialPosition, hidePoint, alpha);
			elapsedTime += Time.deltaTime;

			yield return null;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
		{
			player.velocity = transform.up.normalized * force;
			player.UpdateDirection(player.velocity.x);

			if (lockControl)
			{
				player.input.LockHorizontalControl(lockTime);
			}

			HandlePlayerSnaping(player);

			if (transform.up.y > 0)
			{
				player.state.ChangeState<SpringState>();
			}

			if (hidden)
			{
				StopAllCoroutines();
				StartCoroutine(ShowSpring());
			}

			audio.PlayOneShot(springSound, 0.5f);
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Debug.DrawRay(transform.position, transform.up * 2f);
	}
#endif
}
