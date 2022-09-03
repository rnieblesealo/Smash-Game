using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    [SerializeField] private string idle = "Idle";
    [SerializeField] private string move = "Move";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string hold = "Hold";

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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Start()
    {
        PlayWholeBodyAnimation(idle, 0f);
    }

    private void Update()
    {
        if (!playerController.isGrounded)
            PlayWholeBodyAnimation(jump, 0f);
        else if (playerController.buttonX != 0)
            PlayWholeBodyAnimation(move, 0f);
        else
            PlayWholeBodyAnimation(idle, 0f);
    }
}
