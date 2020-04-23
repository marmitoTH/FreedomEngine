using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public abstract class ItemBox : FreedomObject
{
	[Header("Monitor Components")]
	public GameObject body;
	public GameObject item;
	public ParticleSystem explosionParticle;

	[Header("Monitor Audio")]
	public AudioClip explosionSound;
	public AudioClip collectSound;
	[Range(0, 1)]
	public float collectSoundVolume;

	private new Collider collider;
	protected new AudioSource audio;

	private Player player;

	private Vector3 itemStartPosition;

	private void Start()
	{
		if (!TryGetComponent(out collider))
		{
			collider = gameObject.AddComponent<BoxCollider>();
		}

		if (!TryGetComponent(out audio))
		{
			audio = gameObject.AddComponent<AudioSource>();
		}

		itemStartPosition = item.transform.position;
	}

	public override void OnRespawn()
	{
		StopAllCoroutines();
		body.SetActive(true);
		item.SetActive(true);
		item.transform.position = itemStartPosition;
		collider.enabled = true;
	}

	private void DestroyMonitor(Player player)
	{
		body.SetActive(false);
		collider.enabled = false;
		audio.PlayOneShot(explosionSound);
		explosionParticle.Play();
		StartCoroutine(ReleaseItem(player));
	}

	private IEnumerator ReleaseItem(Player player)
	{
		var position = item.transform.position;
		var t = 0f;

		while (t < 0.5f)
		{
			t += Time.deltaTime;
			position.y += 2f * Time.deltaTime;
			item.transform.position = position;
			yield return null;
		}

		audio.PlayOneShot(collectSound, collectSoundVolume);

		OnCollect(player);

		yield return new WaitForSeconds(.25f);

		item.SetActive(false);
	}

	public override void OnPlayerMotorContact(PlayerMotor motor)
	{
		if (motor.TryGetComponent(out player))
		{
			if (player.attacking)
			{
				if (player.grounded)
				{
					DestroyMonitor(player);
				}
				else
				{
					if (player.velocity.y < 0)
					{
						DestroyMonitor(player);
						motor.velocity.y = -motor.velocity.y;
					}
				}
			}
		}
	}

	protected virtual void OnCollect(Player player) { }
}
