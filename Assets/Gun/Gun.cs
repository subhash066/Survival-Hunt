using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public int maxAmmo = 1;
    public float reloadTime = 1.2f;
    public float bulletSpeed = 50f;

    [Header("References")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public RawImage scopeImage;
    public Text zoomText;
    public GameObject gunModel;
    
    [Tooltip("Assign your Cinemachine Camera here (can be CinemachineCamera or CinemachineVirtualCamera)")]
    public GameObject cameraObject;

    [Header("Zoom")]
    public float normalFOV = 60f;
    public float zoomLerpSpeed = 10f;

    private int zoomLevel = 0; 
    private float targetFOV;
    private CinemachineCamera cm3Camera;
    private CinemachineVirtualCamera cm2Camera;

    [Header("Audio")]
    public AudioClip shotAndReloadSound; 

    private int currentAmmo;
    private bool isReloading = false;
    private AudioSource audioSource;
    private Camera mainCamera;

    void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            normalFOV = mainCamera.fieldOfView;
        }

        InitializeCamera();

        // Try to find gun model if not assigned (usually a child)
        if (gunModel == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("model") || child.GetComponent<Renderer>() != null)
                {
                    gunModel = child.gameObject;
                    break;
                }
            }
        }

        if (scopeImage != null) scopeImage.gameObject.SetActive(false);
        if (zoomText != null) zoomText.text = "1x";
        
        targetFOV = normalFOV;
    }

    void InitializeCamera()
    {
        // If cameraObject is assigned, try to get components from it
        if (cameraObject != null)
        {
            cm3Camera = cameraObject.GetComponent<CinemachineCamera>();
            cm2Camera = cameraObject.GetComponent<CinemachineVirtualCamera>();
        }

        // If still null, try to find in scene
        if (cm3Camera == null && cm2Camera == null)
        {
            cm3Camera = FindFirstObjectByType<CinemachineCamera>();
            if (cm3Camera == null)
            {
                cm2Camera = FindFirstObjectByType<CinemachineVirtualCamera>();
            }
        }

        // Capture initial FOV
        if (cm3Camera != null)
        {
            normalFOV = cm3Camera.Lens.FieldOfView;
        }
        else if (cm2Camera != null)
        {
            normalFOV = cm2Camera.m_Lens.FieldOfView;
        }
        else
        {
            Debug.LogWarning("No Cinemachine camera found! Zoom will not be visible if Cinemachine is active.");
        }
        
        targetFOV = normalFOV;
    }

    void Update()
    {
        HandleZoom();
        if (isReloading) return;

        if (Input.GetButtonDown("Fire1") && currentAmmo > 0)
        {
            Shoot();
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        currentAmmo--;
        if (audioSource != null && shotAndReloadSound != null)
            audioSource.PlayOneShot(shotAndReloadSound);

        if (bulletPrefab != null)
        {
            Vector3 spawnPosition = mainCamera != null ? mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1.2f)) : firePoint.position;
            Quaternion spawnRotation = mainCamera != null ? Quaternion.LookRotation(mainCamera.transform.forward) : firePoint.rotation;
            Vector3 shootDirection = mainCamera != null ? mainCamera.transform.forward : firePoint.forward;

            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);
            bullet.tag = "Bullet";
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.linearVelocity = shootDirection * bulletSpeed;
            }
            Destroy(bullet, 3f);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void HandleZoom()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            zoomLevel = Mathf.Min(2, zoomLevel + 1);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            zoomLevel = Mathf.Max(0, zoomLevel - 1);
        }

        float zoomFactor = Mathf.Pow(2, zoomLevel);
        targetFOV = normalFOV / zoomFactor;

        if (zoomText != null) zoomText.text = zoomFactor + "x";

        // Apply to the detected camera type
        if (cm3Camera != null)
        {
            var lens = cm3Camera.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * zoomLerpSpeed);
            cm3Camera.Lens = lens;
        }
        else if (cm2Camera != null)
        {
            cm2Camera.m_Lens.FieldOfView = Mathf.Lerp(cm2Camera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * zoomLerpSpeed);
        }
        else if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * zoomLerpSpeed);
        }

        if (scopeImage != null)
            scopeImage.gameObject.SetActive(zoomLevel > 0);

        // Hide gun model when zoomed
        if (gunModel != null)
            gunModel.SetActive(zoomLevel == 0);
    }
}
