using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Jump Panel")]
[RequireComponent(typeof(Collider))]
public class JumpPanel : MonoBehaviour
{
    public float distance;
    public float height;
    public AudioClip panelClip;

    private new AudioSource audio;
    private new Collider collider;

    private void Start()
    {
        if (!TryGetComponent(out audio))
        {
            audio = gameObject.AddComponent<AudioSource>();
        }

        if (!TryGetComponent(out collider))
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }

        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            audio.PlayOneShot(panelClip, 0.5f);
            player.state.ChangeState<WalkPlayerState>();
            player.velocity = transform.right * distance + transform.up * height;
        }
    }
}
