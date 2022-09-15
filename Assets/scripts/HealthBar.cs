using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private PlayerController trackedPlayer;
    private Graphic[] graphics; // Cache of all GUI parts of this health bar

    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text ammoText;
    [SerializeField] private float smoothTime;
    [SerializeField] private float alphaCrossfadeTime;

    private bool isTransparent = false;

    public void Configure(PlayerController trackedPlayer)
    {
        this.trackedPlayer = trackedPlayer;
    }

    private void Awake()
    {
        graphics = GetComponentsInChildren<Graphic>();
    }

    private void Update()
    {
        if (!trackedPlayer)
            return;

        // Update health bar fill amount
        transform.position = Camera.main.WorldToScreenPoint(trackedPlayer.UIAnchor.position); // Health bar follows tracked player's UI anchor's world position
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, (float)trackedPlayer.currentHealth / trackedPlayer.settings.maxHealth, smoothTime * Time.deltaTime);

        if (ammoText)
            ammoText.text = (trackedPlayer.gun && !trackedPlayer.gun.isReloading) ? (trackedPlayer.gun.currentAmmo + "/" + trackedPlayer.gun.currentReserve) : "RELOADING";

        // Make healthbar transparent when player is dead for effect
        if (trackedPlayer.isDead && !isTransparent)
        {
            for (int i = 0; i < graphics.Length; ++i)
                graphics[i].CrossFadeAlpha(0.25f, alphaCrossfadeTime, false);

            isTransparent = true;
        }

        else if (!trackedPlayer.isDead && isTransparent)
        {
            for (int i = 0; i < graphics.Length; ++i)
                graphics[i].CrossFadeAlpha(1, alphaCrossfadeTime, true);

            isTransparent = false;
        }
    }
}
