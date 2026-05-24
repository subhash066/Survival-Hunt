using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaterSound : MonoBehaviour
{
    public Transform player;            // Reference to the player
    public AudioClip waterClip;         // Slot for water sound
    public float minDistance = 2f;      // Distance at which sound is max volume
    public float maxDistance = 15f;     // Distance at which sound is min volume
    public float minVolume = 0f;        // Lowest volume
    public float maxVolume = 1f;        // Highest volume

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (waterClip != null)
        {
            audioSource.clip = waterClip;
            audioSource.loop = true;       // Keep water sound looping
            audioSource.playOnAwake = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No water sound clip assigned!");
        }
    }

    void Update()
    {
        if (player == null || audioSource.clip == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        // Normalize distance between min and max
        float t = Mathf.InverseLerp(maxDistance, minDistance, distance);

        // Lerp volume based on distance
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, t);
    }
}
