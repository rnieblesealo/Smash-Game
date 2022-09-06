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

    private int currentReserve;
    private int currentAmmo;

    private const float maxShootDistance = 500;
    private const float missTargetDistance = maxShootDistance / 2;

    [HideInInspector] public float nextFireTime = 0f;
    [HideInInspector] public float reloadStopTime = 0f;

    public LayerMask notShootable;

    public void Shoot()
    {
        if (currentAmmo == 0 || Time.time < nextFireTime)
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

    public void Reload()
    {
        if (currentAmmo == maxAmmo)
            return;

        int neededAmmo = maxAmmo - currentAmmo;
        int reloadedAmmo = currentReserve >= neededAmmo ? neededAmmo : currentReserve;

        currentReserve -= reloadedAmmo;
        currentAmmo += reloadedAmmo;
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
}
