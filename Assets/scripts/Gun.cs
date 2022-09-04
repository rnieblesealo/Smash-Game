using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerController playerController;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Sounds")]
    [SerializeField] private AudioClip shoot;


    [Header("Settings")]
    [SerializeField] private int maxReserve;
    [SerializeField] private int currentReserve;
    [SerializeField] private int maxAmmo;
    [SerializeField] private int currentAmmo;

    private const float maxShootDistance = 500;
    private const float missTargetDistance = maxShootDistance / 2;

    public LayerMask notShootable;

    private void Shoot()
    {
        if (currentAmmo == 0)
            return;

        currentAmmo--;

        Ray shot = new Ray(shootPoint.position, shootPoint.forward);
        Vector3 target;

        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, maxShootDistance, notShootable))
            target = hit.point;
        else
            target = shot.GetPoint(missTargetDistance);

        Bullet bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity).GetComponent<Bullet>();

        bullet.origin = shootPoint.position;
        bullet.target = target;

        audioSource.PlayOneShot(shoot);
    }

    private void Reload()
    {
        if (currentAmmo == maxAmmo)
            return;

        int neededAmmo = maxAmmo - currentAmmo;
        int reloadedAmmo = currentReserve >= neededAmmo ? neededAmmo : currentReserve;

        currentReserve -= reloadedAmmo;
        currentAmmo += reloadedAmmo;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponentInParent<PlayerController>();

        currentAmmo = maxAmmo;
        currentReserve = maxReserve;
    }

    private void Update()
    {
        if (Input.GetKeyDown(playerController.shoot))
            Shoot();

        else if (Input.GetKeyDown(playerController.reload))
            Reload();
    }
}
