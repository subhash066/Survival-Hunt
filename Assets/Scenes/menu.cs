using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    [Header("UI Raw Image References")]
    public RawImage playButton;
    public RawImage quitButton;

    [Header("Scene Management")]
    public string startSceneName = "main"; // ONLY scene name (not path)

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip clickSound;

    private AudioSource bgMusicSource;
    private AudioSource clickSoundSource;

    private void Awake()
    {
        // Background Music setup
        bgMusicSource = gameObject.AddComponent<AudioSource>();
        bgMusicSource.clip = backgroundMusic;
        bgMusicSource.loop = true;
        bgMusicSource.playOnAwake = true;
        bgMusicSource.volume = 0.5f;

        // Click sound setup
        clickSoundSource = gameObject.AddComponent<AudioSource>();
        clickSoundSource.loop = false;
        clickSoundSource.playOnAwake = false;
    }

    private void Start()
    {
        if (bgMusicSource.clip != null)
            bgMusicSource.Play();

        if (playButton != null)
            AddEventTrigger(playButton.gameObject, OnPlayClicked);

        if (quitButton != null)
            AddEventTrigger(quitButton.gameObject, OnQuitClicked);
    }

    private void AddEventTrigger(GameObject target, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = target.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };

        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    private void OnPlayClicked(BaseEventData data)
    {
        PlayClickSound();

        if (string.IsNullOrEmpty(startSceneName))
        {
            Debug.LogError("Menu: startSceneName is empty. Set it to the build scene name of the main scene.");
            return;
        }

        Debug.Log($"Loading scene '{startSceneName}'...");
        SceneManager.LoadScene(startSceneName);
    }

    private void OnQuitClicked(BaseEventData data)
    {
        PlayClickSound();
        Invoke(nameof(ExecuteQuit), 0.4f);
    }

    private void PlayClickSound()
    {
        if (clickSoundSource != null && clickSound != null)
            clickSoundSource.PlayOneShot(clickSound);
    }

    private void ExecuteQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}