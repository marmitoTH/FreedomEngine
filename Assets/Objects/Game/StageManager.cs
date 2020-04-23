using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[AddComponentMenu("Freedom Engine/Game/Stage Manager")]
public class StageManager : MonoBehaviour
{
    private static StageManager instance;

    public static StageManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<StageManager>();
                instance.StartSingleton();
            }

            return instance;
        }
    }

    [Header("UI Elements")]
    [SerializeField] private GameObject titleCard = null;
    [SerializeField] private Image fader = null;
    [SerializeField] private float fadeDelay = 0;
    [SerializeField] private float fadeTime = 0;
    
    [Header("Stage Settings")]
    [SerializeField] private Player player = null;
    [SerializeField] private string nextStage = "";
    public Rect bounds;

    [Header("Stage Music")]
    [SerializeField] private AudioClip song = null;
    [Range(0f, 1f)]
    [SerializeField] private float songVolume = 1f;

    private Vector3 startPoint;
    private Quaternion startRotation;
    private float startTime;

    private new AudioSource audio;

    private void StartSingleton()
    {
        if (!TryGetComponent(out audio))
        {
            audio = gameObject.AddComponent<AudioSource>();
        }

        startPoint = player.transform.position;
        startRotation = player.transform.rotation;
        StartStage();
    }

    public void StartStage()
    {
        StartCoroutine(InitializeStage());
    }

    public void StartSong()
    {
        audio.Stop();
        audio.loop = true;
        audio.clip = song;
        audio.volume = songVolume;
        audio.Play();
    }

    public void SetStartState(Vector3 point, Quaternion rotation, float time)
    {
        startPoint = point;
        startRotation = rotation;
        startTime = time;
    }

    public void ChangeSong(AudioClip song, float volume = 1, bool loop = false)
    {
        audio.Stop();
        audio.clip = song;
        audio.loop = loop;
        audio.volume = volume;
        audio.Play();
    }

    public void Restart(bool dead = true)
    {
        StartCoroutine(FadeOut(dead));
    }

    public void FinishStage()
    {
        StartCoroutine(EndStage());
    }

    private IEnumerator InitializeStage()
    {
        ScoreManager.Instance.stopTimer = true;
        ScoreManager.Instance.ResetTimer();
        titleCard.SetActive(true);
        var titleCardDuration = titleCard.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
        StartSong();
        player.disableInput = true;
        player.Respawn(startPoint, startRotation);
        ObjectsManager.Instance.RespawnFreedomObjects();
        yield return new WaitForSeconds(titleCardDuration);
        ScoreManager.Instance.stopTimer = false;
        ScoreManager.Instance.time = startTime;
        player.disableInput = false;
        titleCard.SetActive(false);
    }

    private IEnumerator FadeOut(bool dead)
    {
        yield return new WaitForSeconds(fadeDelay);

        var elapsedTime = 0f;
        var color = fader.color;

        while(elapsedTime < fadeTime)
        {
            var alpha = elapsedTime / fadeTime;

            color.a = Mathf.Lerp(0, 1, alpha);
            audio.volume = Mathf.Lerp(songVolume, 0, alpha);
            fader.color = color;
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        if (dead)
        {
            ScoreManager.Instance.Lifes--;
        }

        fader.color = new Color(0, 0, 0, 0);
        ScoreManager.Instance.ResetTimer();
        ScoreManager.Instance.Rings = 0;
        StartStage();
    }
    
    private IEnumerator EndStage()
    {
        ScoreManager.Instance.stopTimer = true;

        yield return new WaitForSeconds(fadeDelay);

        var elapsedTime = 0f;
        var color = fader.color;

        while (elapsedTime < fadeTime)
        {
            var alpha = elapsedTime / fadeTime;

            color.a = Mathf.Lerp(0, 1, alpha);
            audio.volume = Mathf.Lerp(songVolume, 0, alpha);
            fader.color = color;
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        player.disableInput = true;
        ScoreManager.Instance.ResetTimer();
        SceneManager.LoadScene(nextStage);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
