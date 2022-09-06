using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private PlayerController trackedPlayer;

    [SerializeField] private Image fill;
    [SerializeField] private float smoothTime;

    public void Configure(PlayerController trackedPlayer)
    {
        this.trackedPlayer = trackedPlayer;
    }

    private void Update()
    {
        if (!trackedPlayer)
            return;

        transform.position = Camera.main.WorldToScreenPoint(trackedPlayer.UIAnchor.position);

        fill.fillAmount = Mathf.Lerp(fill.fillAmount, trackedPlayer.currentHealth / trackedPlayer.maxHealth, smoothTime * Time.deltaTime);
    }
}
