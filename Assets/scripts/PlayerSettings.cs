using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Settings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Gameplay Settings")]
    public int maxHealth;
    public float movementSpeed;
    public float jumpHeight;
    public float respawnDelay;

    [Header("Jump Settings")]
    public float gravity;
    public float jumpDistribution;
    public float maxJumpMultiplier;
    public float jumpHoldIncrement;
    public float jumpPhaseDeadzone;
    public float downwardPhaseVelocity;
    public float fastFallStrength;

    [Header("Collision & Range Settings")]
    public float itemScanRadius;
    public float groundScanRadius;

    [Header("Throw Physics Settings")]
    public float throwTorque;
    public float throwForce;
    public float upThrowDistribution;

    [Header("Layer Masks")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPickup;

    [Header("Other Settings")]
    public float smoothTime;
}
