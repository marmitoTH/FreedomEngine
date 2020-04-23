using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Checkpoint")]
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    public Transform startPoint;
    public AudioClip activateClip;
    public Animator animator;

    private bool triggered;

    private new AudioSource audio;

    private void Start()
    {
        if (!TryGetComponent(out audio))
        {
            audio = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            animator.SetBool("Activated", true);
            audio.PlayOneShot(activateClip, 0.5f);
            StageManager.Instance.SetStartState(startPoint.position, Quaternion.identity, ScoreManager.Instance.time);
        }
    }
}
