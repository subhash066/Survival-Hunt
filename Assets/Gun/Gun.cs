using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float damage = 10f;
    public float fireRate = 10f;
    public int maxAmmo = 30;
    public float reloadTime = 2f;
    public float bulletSpeed = 50f;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;

    [Header("References")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject ammoTextObject;

    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private bool isPickedUp = false;
    private AudioSource audioSource;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        currentAmmo = maxAmmo;
        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        UpdateAmmoUI();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void UpdateAmmoUI()
    {
        if (ammoTextObject == null) return;

        Text legacyText = ammoTextObject.GetComponent<Text>();
        if (legacyText != null)
        {
            legacyText.text = $"Ammo: {currentAmmo}/{maxAmmo}";
            legacyText.enabled = isPickedUp;
            return;
        }

#if UNITY_EDITOR
        TMPro.TextMeshProUGUI tmpText = ammoTextObject.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"Ammo: {currentAmmo}/{maxAmmo}";
            tmpText.enabled = isPickedUp;
        }
#endif
    }

    void OnEnable()
    {
        isReloading = false;
    }

    void Update()
    {
        if (!isPickedUp) return;

        if (isReloading)
            return;

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    public void PickUp(Transform player, Transform holdPoint)
    {
        isPickedUp = true;
        transform.SetParent(holdPoint != null ? holdPoint : player);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        UpdateAmmoUI();
    }

    public void Drop()
    {
        isPickedUp = false;
        transform.SetParent(originalParent);

        if (originalParent == null)
        {
            transform.position += Vector3.up;
            transform.rotation = originalRotation;
        }
        else
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Camera.main.transform.forward * 5f, ForceMode.Impulse);
        }

        if (ammoTextObject != null) ammoTextObject.SetActive(false);
    }

    void Shoot()
    {
        currentAmmo--;
        UpdateAmmoUI();

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.forward * bulletSpeed;
            }
        }
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
    }
}