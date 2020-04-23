using UnityEngine;
using System.Collections;

[AddComponentMenu("Freedom Engine/Objects/Player/Player Skin")]
public class PlayerSkin : MonoBehaviour
{
	public enum Mouth { Center, Dead, Left, Right }

	[Header("Skin Animator")]
	public Animator animator;

	[Header("Transforms")]
	public Transform root;

	[Header("Game Objects")]
	public GameObject skin;
	public GameObject ball;

	[Header("Renderers")]
	public SkinnedMeshRenderer skinRenederer;
	public MeshRenderer ballRenderer;

	[Header("Skin Mouths")]
	public Transform mouthCenter;
	public Transform mouthDead;
	public Transform mouthLeft;
	public Transform mouthRight;

	private Transform currentActivatedBody;
	private readonly Transform[] mouths = new Transform[4];
	private int currentActivatedMouth;

	private IEnumerator blinkCoroutine;

	private void Start()
	{
		InitializeMouths();
		InitializeBody();
	}

	private void LateUpdate()
	{
		foreach (Transform mouth in mouths)
		{
			mouth.localScale = Vector3.zero;
		}

		mouths[currentActivatedMouth].localScale = Vector3.one;
	}

	private void InitializeMouths()
	{
		mouths[0] = mouthCenter;
		mouths[1] = mouthDead;
		mouths[2] = mouthLeft;
		mouths[3] = mouthRight;
	}

	private void InitializeBody()
	{
		skin.transform.localScale = Vector3.one;
		ball.transform.localScale = Vector3.zero;
		currentActivatedBody = skin.transform;
	}

	public void ActiveBall(bool value = true)
	{
		currentActivatedBody.localScale = Vector3.zero;
		currentActivatedBody = value ? ball.transform : skin.transform;
		currentActivatedBody.localScale = Vector3.one;
	}

	public void ChangeMouth(Mouth mouth)
	{
		currentActivatedMouth = (int)mouth;
	}

	public void SetEulerY(float angle)
	{
		var euler = root.eulerAngles;
		euler.y = angle;
		root.eulerAngles = euler;
	}

	public void SetEulerZ(float angle)
	{
		var euler = root.eulerAngles;
		euler.z = angle;
		root.eulerAngles = euler;
	}

	public void StartBlinking(float duration)
	{
		blinkCoroutine = Blink(duration);
		StartCoroutine(blinkCoroutine);
	}

	public void StopBlinking()
	{
		if (blinkCoroutine != null)
		{
			StopCoroutine(blinkCoroutine);
			currentActivatedBody.localScale = Vector3.one;
		}
	}

	private IEnumerator Blink(float duration)
	{
		duration += Time.time;

		while (Time.time < duration)
		{
			yield return new WaitForSeconds(0.1f);
			currentActivatedBody.localScale = Vector3.zero;
			yield return new WaitForSeconds(0.1f);
			currentActivatedBody.localScale = Vector3.one;
		}
	}
}