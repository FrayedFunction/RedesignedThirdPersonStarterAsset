using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MccDev260.InputData;

public class PlayerFSMController : MonoBehaviour
{
    public GameObject SkeletonObj;
    public AnimRigController RigController;
    
    public InputManager inputManager;
    
    public CharacterController AttachedController;
    public Animator Animator;
    public ObjOrientation ObjOrientation;

    // Structure later.
    #region Data
    [Header("Player")]
    public GameObject PlayerObject;

    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;
    #endregion

    public PlayerStateManager StateManager;
    public GameInputMap gameInputMap;

    public void Awake()
    {
        StateManager = new PlayerStateManager(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        StateManager.SetState(PlayerState.Controllable);
    }

    // Update is called once per frame
    void Update()
    {
        gameInputMap = inputManager.GetGameMap;
        StateManager.CurrentState?.OnUpdate();
    }

    private void OnDestroy()
    {
       StateManager.OnDestroy();
    }

    /// <summary>
    /// Apply force to obj
    /// </summary>
    /// <param name="f"></param>
    public void AddForce(float f)
    {
        AddForce(new Vector3(f,f,f));
    }

    public void AddForce(Vector3 force) 
    {
        if (!StateManager.CompareCurrentState(PlayerState.Ragdoll))
            StateManager.SetState(PlayerState.Ragdoll);

        var state = StateManager.CurrentState as RagdollState;
        state.AddForce(force);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(AttachedController.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(AttachedController.center), FootstepAudioVolume);
        }
    }
}
