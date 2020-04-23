using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[AddComponentMenu("Freedom Engine/Game/Title Screen")]
public class TitleScreen : MonoBehaviour
{
    public Image fader;
    public GameObject startButton;
    public string nextScene;

    public AudioClip startClip;
    public AudioSource startMusicSource;

    public float startUnlockTime;
    public float startDelay;
    public float fadeInDuration;
    public float fadeOutDuration;

    private bool locked;
    private bool started;
    private float startTimer;

    private new AudioSource audio;

    private void Start()
    {
        InitializeAudio();
        InitializeFader();
        InitializeStartScreen();
    }

    private void InitializeAudio()
    {
        if (!TryGetComponent(out audio))
        {
            audio = gameObject.AddComponent<AudioSource>();
        }
    }

    private void InitializeFader()
    {
        var faderColor = fader.color;
        faderColor.a = 1f;
        fader.color = faderColor;
        StartCoroutine(Fade(1, 0, fadeInDuration));
    }

    private void InitializeStartScreen()
    {
        locked = true;
        started = false;
        startTimer = 0f;
        startButton.SetActive(false);
    }

    private void Update()
    {
        if (!started)
        {
            if (locked)
            {
                startTimer += Time.deltaTime;

                if (startTimer >= startUnlockTime)
                {
                    locked = false;
                    startTimer = 0;
                    startButton.SetActive(true);
                }
            }
            else
            {
                if (Input.GetButtonDown("Start"))
                {
                    started = true;
                    startButton.SetActive(false);
                    StartCoroutine(StartGame());
                    audio.PlayOneShot(startClip);
                }
            }
        }
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(startDelay);

        StartCoroutine(Fade(0, 1, fadeOutDuration));

        var elapsedTime = 0f;
        var initialVolume = startMusicSource.volume;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;

            var alpha = elapsedTime / fadeOutDuration;
            startMusicSource.volume = Mathf.Lerp(initialVolume, 0, alpha);

            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        var faderColor = fader.color;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            var alpha = elapsedTime / duration;
            faderColor.a = Mathf.Lerp(from, to, alpha);
            fader.color = faderColor;

            yield return null;
        }
    }
}
