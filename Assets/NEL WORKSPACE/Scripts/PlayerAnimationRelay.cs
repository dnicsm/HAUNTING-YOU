using UnityEngine;

public class PlayerAnimationRelay : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    public void AnimationEvent_EndJump() => playerController?.AnimationEvent_EndJump();
    public void AnimationEvent_EndDodge() => playerController?.AnimationEvent_EndDodge();
    public void AnimationEvent_EndHurt() => playerController?.AnimationEvent_EndHurt();
    public void AnimationEvent_EndBurn() => playerController?.AnimationEvent_EndBurn();
}