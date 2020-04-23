using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Goal Plate")]
public class GoalPlate : FreedomObject
{
    public Animator animator;
    public AudioClip plateClip;

    private bool activated;

    private new AudioSource audio;

    private void Start()
    {
        if (!TryGetComponent(out audio))
        {
            audio = gameObject.AddComponent<AudioSource>();
        }
    }

    public override void OnRespawn()
    {
        activated = false;
        animator.SetBool("Activated", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;
            animator.SetBool("Activated", true);
            audio.PlayOneShot(plateClip, 0.5f);
            StageManager.Instance.FinishStage();
        }
    }
}
