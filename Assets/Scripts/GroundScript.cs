using UnityEngine;

public class GroundScript : MonoBehaviour
{
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float volume = 0.5f;

    private AudioSource audioSource;

    void Start()
    {
        if (backgroundMusic != null)
        {
            // Add an AudioSource component if it doesn't exist
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Configure the AudioSource
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.playOnAwake = false; // We control it via script
            
            // Start playing
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("GroundScript: No Background Music assigned! Drag an AudioClip into the slot in the Inspector.");
        }
    }
}
