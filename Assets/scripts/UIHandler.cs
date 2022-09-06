using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    private PlayerController[] players;

    [SerializeField] private GameObject healthBarPrefab;

    private void Awake()
    {
        players = FindObjectsOfType<PlayerController>();
    }

    private void Start()
    {
        for (int i = 0; i < players.Length; ++i)
        {
            HealthBar healthBar = Instantiate(healthBarPrefab, parent: transform).GetComponent<HealthBar>();

            healthBar.Configure(players[i]);
        }
    }
}
