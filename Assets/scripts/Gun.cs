using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private AudioSource audioSource;
    private ParticleSystem muzzleFlash;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform heldTransform;
    [SerializeField] private Transform holsteredTransform;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Sounds")]
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip draw;
    [SerializeField] private AudioClip holster;

    [Header("Settings")]
    [SerializeField] private int maxReserve;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float fireRate;
    [SerializeField] private float bloom;
    [SerializeField] private float reloadDuration;

    [HideInInspector] public int currentReserve;
    [HideInInspector] public int currentAmmo;

    public bool isReloading = false;

    private const float maxShootDistance = 500;
    private const float missTargetDistance = maxShootDistance / 2;

    [HideInInspector] public float nextFireTime = 0f;
    [HideInInspector] public float nextReloadTime = 0f;

    public LayerMask notShootable;

    public void Shoot()
    {
        if (currentAmmo == 0 || Time.time < nextFireTime || isReloading)
            return;

        currentAmmo--;

        Ray shot = new Ray(shootPoint.position, shootPoint.forward + shootPoint.up * Random.Range(-bloom, bloom));
        Vector3 target;

        if (Physics.Raycast(shot, out RaycastHit hit, maxShootDistance, notShootable))
            target = hit.point;
        else
            target = shot.GetPoint(missTargetDistance);

        Bullet bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity).GetComponent<Bullet>();

        bullet.Configure(shootPoint.position, target, (GetComponentInParent<PlayerController>() ? GetComponentInParent<PlayerController>().transform : transform));

        nextFireTime = Time.time + 1f / fireRate;

        audioSource.PlayOneShot(shoot);
        muzzleFlash.Play();
    }

    public void BeginReload()
    {
        if (isReloading || currentAmmo == maxAmmo)
            return;

        nextReloadTime = Time.time + reloadDuration;
        print(nextReloadTime);
        isReloading = true;

        print("Beginning Reload!");
    }

    private void Reload()
    {
        int neededAmmo = maxAmmo - currentAmmo;
        int reloadedAmmo = currentReserve >= neededAmmo ? neededAmmo : currentReserve;

        currentReserve -= reloadedAmmo;
        currentAmmo += reloadedAmmo;

        isReloading = false;

        print("Reloaded!");
    }

    public void Holster()
    {
        gameObject.transform.SetParent(holsteredTransform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;

        audioSource.PlayOneShot(holster);
    }

    public void Draw()
    {
        gameObject.transform.SetParent(heldTransform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;

        audioSource.PlayOneShot(draw);
    }

    public void Reset()
    {
        /* On reset:
         * Restore ammo
         * Clear reloading state
         */

        // Reset ammo
        currentAmmo = maxAmmo;
        currentReserve = maxReserve;

        // Clear reloading state
        isReloading = false;
        nextReloadTime = 0;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        muzzleFlash = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        currentReserve = maxReserve;
    }

    private void Update()
    {
        if (Time.time > nextReloadTime && nextReloadTime > 0 && isReloading)
        {
            Reload();
        }
    }
}
