using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;
    private PlayerController playerController;

    [Header("Animator Parameters")]
    [SerializeField] private string idle = "Idle";
    [SerializeField] private string move = "Move";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string hold = "Hold";
    [SerializeField] private string gunHold = "Gun Hold";

    [Header("Sounds")]
    public AudioClip[] sounds;

    [Header("Other")]
    [SerializeField] private Transform headObject;
    [SerializeField] private Transform headTarget;
    [SerializeField] private Transform walkParticles;

    private string currentBodyAnimation;
    private string currentArmsAnimation;
    private string currentLegsAnimation;

    public enum BodyPart { Body, Arms, Legs }; // Make sure to keep layers of character animator in this order!

    public void PlayAnimation(BodyPart bodyPart, string newAnimation, float crossfadeTime = 0.25f)
    {
        // Sets animation of specified body part
        switch (bodyPart)
        {
            case BodyPart.Body:
                if (currentBodyAnimation == newAnimation)
                    return;
                animator.CrossFade(newAnimation, crossfadeTime, (int)BodyPart.Body);
                currentBodyAnimation = newAnimation;
                break;
            case BodyPart.Arms:
                if (currentArmsAnimation == newAnimation)
                    return;
                animator.CrossFade(newAnimation, crossfadeTime, (int)BodyPart.Arms);
                currentArmsAnimation = newAnimation;
                break;
            case BodyPart.Legs:
                if (currentLegsAnimation == newAnimation)
                    return;
                animator.CrossFade(newAnimation, crossfadeTime, (int)BodyPart.Legs);
                currentLegsAnimation = newAnimation;
                break;
        }
    }

    public void PlayWholeBodyAnimation(string newAnimation, float crossfadeTime = 0.25f)
    {
        if (currentBodyAnimation != newAnimation)
        {
            animator.CrossFade(newAnimation, crossfadeTime, (int)BodyPart.Body);
            currentBodyAnimation = newAnimation;
        }

        if (currentArmsAnimation != newAnimation)
        {
            animator.CrossFade(newAnimation, crossfadeTime, (int)BodyPart.Arms);
            currentArmsAnimation = newAnimation;
        }

        if (currentLegsAnimation != newAnimation)
        {
            animator.CrossFade(newAnimation, crossfadeTime, (int)BodyPart.Legs);
            currentLegsAnimation = newAnimation;
        }
    }

    public void PlaySound(int index)
    {
        audioSource.PlayOneShot(sounds[index]);
    }

    private void LookAtTarget()
    {
        if (headObject == null)
            return;

        // TODO Make head look at target, should be other player's head, implement search function for this later
        // TODO Add some sort of constraint to head angle, since head can turn past other half of body
        else if (headTarget == null)
        {
            // Head rotation should be none if no target object is present
            if (headObject.transform.localRotation != Quaternion.identity)
                headObject.transform.localRotation = Quaternion.identity;

            return;
        }

        headObject.transform.LookAt(headTarget);
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Start()
    {
        PlayWholeBodyAnimation(idle, 0f);
    }

    private void Update()
    {
        LookAtTarget();

        /*
         * If holding gun, arms are always at hold position
         */

        if (playerController.isHoldingGun)
        {
            PlayAnimation(BodyPart.Arms, gunHold, 0f);

            if (!playerController.isGrounded)
            {
                PlayAnimation(BodyPart.Body, jump, 0f);
                PlayAnimation(BodyPart.Legs, jump, 0f);
            }

            else if (playerController.buttonX != 0)
            {
                PlayAnimation(BodyPart.Body, move, 0f);
                PlayAnimation(BodyPart.Legs, move, 0f);
            }

            else
            {
                PlayAnimation(BodyPart.Body, idle, 0f);
                PlayAnimation(BodyPart.Legs, idle, 0f);
            }
        }

        else
        {
            if (!playerController.isGrounded)
                PlayWholeBodyAnimation(jump, 0f);
            else if (playerController.buttonX != 0)
                PlayWholeBodyAnimation(move, 0f);
            else
                PlayWholeBodyAnimation(idle, 0f);
        }

        // Handle walk particles
        Vector3 walkParticleRotation = Vector3.up * (playerController.lastButtonX < 0 ? 0 : 180);
        ParticleSystem.EmissionModule walkParticlesEmission = walkParticles.GetComponent<ParticleSystem>().emission;

        walkParticles.localRotation = Quaternion.Euler(walkParticleRotation);
        walkParticlesEmission.enabled = playerController.buttonX != 0 && playerController.isGrounded;
    }
}
