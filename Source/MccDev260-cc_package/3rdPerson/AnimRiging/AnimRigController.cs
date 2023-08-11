using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
/* TODO:
 *  - Add DisableRigs method.
 * 
 */

public class AnimRigController : MonoBehaviour
{
    public enum RigType
    {
        Head,
        ArmLeft,
        ArmRight,
        All,
    }

    [Header("Debug")]
    public Transform testTarget;

    #region Data
    #region Serialized
    [Header("Look")]
    [SerializeField] private Rig headRig;
    [SerializeField] private Transform lookSource;
    [Range(0, 1)]
    [SerializeField] private float lookWeight = 1f;

    [Header("Left Arm")]
    [SerializeField] private Rig leftArmRig;
    [SerializeField] private Transform leftArmSource;
    [Range(0, 1)]
    [SerializeField] private float leftArmWeight = 1f;

    [Header("Right Arm")]
    [SerializeField] private Rig rightArmRig;
    [SerializeField] private Transform rightArmSource;
    [Range(0, 1)]
    [SerializeField] private float rightArmWeight = 1f;

    [Header("Look Tracking Settings")]
    [SerializeField] private Transform lookTrackedObj;
    [SerializeField] private bool isLookTracking = false;
    [SerializeField] private float lookTrackingDelay = 0.15f;
    [SerializeField] private float lookReactionThreshold = 0.05f;

    [Header("Right Arm Tracking Settings")]
    [SerializeField] private Transform rightArmTrackedObj;
    [SerializeField] private bool isTrackingRightArm = false;
    [SerializeField] private float rightArmTrackDelay = 0.15f;
    [SerializeField] private float rightArmReacitonThreshold = 0.05f;

    [Header("Right Arm Tracking Settings")]
    [SerializeField] private Transform leftArmTrackedObj;
    [SerializeField] private bool isTrackingLeftArm = false;
    [SerializeField] private float leftArmTrackDelay = 0.15f;
    [SerializeField] private float leftArmReacitonThreshold = 0.05f;

    [Header("Reset Settings")]
    public float TEST_resetTime = 0.2f;
    #endregion

    #region Members
    Vector3 _origLookSourcePos;
    Vector3 _origRightArmSourcePos;
    Vector3 _origLeftArmSourcePos;

    Vector3 _lookSourcePosLastFrame;
    Vector3 _rightArmSourcePosLastFrame;
    Vector3 _leftArmSourcePosLastFrame;
    #endregion
    #endregion

    private void Awake()
    {
        _origLookSourcePos = lookSource.localPosition;
        _origRightArmSourcePos = rightArmSource.localPosition;
        _origLeftArmSourcePos = leftArmSource.localPosition;
    }

    private void Start()
    {
        // Testing
        TrackObj(RigType.Head, testTarget, weight: 100f);
    }

    // Update is called once per frame
    void Update()
    {
        headRig.weight = lookWeight;
        leftArmRig.weight = leftArmWeight;
        rightArmRig.weight = rightArmWeight;

        if (lookTrackedObj != null)
            UpdateLookTracking();
        
        if (rightArmTrackedObj != null)
            UpdateRightArmTracking();

        if (leftArmTrackedObj != null) 
            UpdateLeftArmTracking();

    }

    #region Public Funcs
    /// <summary>
    /// Sets the weight for a specific rig to a new value over a given amount of time.
    /// </summary>
    /// <param name="rig">The type of rig to set the weight for.</param>
    /// <param name="weight">The new weight value.</param>
    /// <param name="time">The amount of time in seconds it should take to complete the weight transition.</param>
    /// <remarks>Does not need to be called in Update.</remarks>
    public void SetWeight(RigType rig, float weight, float time = 0.3f)
    {
        switch (rig)
        {
            case RigType.Head:
                var startWeight = lookWeight;
                StartCoroutine(SetWeight(startWeight, weight, time, res => lookWeight = res));
                break;

            case RigType.ArmLeft:
                startWeight = leftArmWeight;
                StartCoroutine(SetWeight(startWeight, weight, time, res => leftArmWeight = res));
                break;

            case RigType.ArmRight:
                startWeight = rightArmWeight;
                StartCoroutine(SetWeight(startWeight, weight, time, res => rightArmWeight = res));
                break;

            case RigType.All:
                SetWeight(RigType.Head, weight, time);
                SetWeight(RigType.ArmRight, weight, time);
                SetWeight(RigType.ArmLeft, weight, time);
                break;
#if UNITY_EDITOR
            default: throw new NotImplementedException($"{nameof(AnimRigController)}, {nameof(SetWeight)}: RigType.{rig} has no associated case!");
#endif
        }
    }

    /// <summary>
    /// Tracks the specified object with the given weight and delay.
    /// </summary>
    /// <param name="rig">Rig to override.</param>
    /// <param name="target">Target object to track.</param>
    /// <param name="delay">The delay before tracking begins, in seconds.</param>
    /// <param name="weight">Rig weight.</param>
    /// <remarks>Does not need to be called in Update.</remarks>
    public void TrackObj(RigType rig, Transform target, float weight, float delay = 0.3f)
    {
        switch (rig)
        {
            case RigType.Head:
                if (lookWeight != weight)
                {
                    var currentWeight = lookWeight;
                    StartCoroutine(SetWeight(currentWeight, weight, 0.2f, res => lookWeight = res));
                }
                break;

            case RigType.ArmLeft:
                if (leftArmWeight != weight)
                {
                    var currentWeight = leftArmWeight;
                    StartCoroutine(SetWeight(currentWeight, weight, 0.2f, res => leftArmWeight = res));
                }
                break;
            case RigType.ArmRight:
                if (rightArmWeight != weight)
                {
                    var currentWeight = rightArmWeight;
                    StartCoroutine(SetWeight(currentWeight, weight, 0.2f, res => rightArmWeight = res));
                }
                break;
#if UNITY_EDITOR
            default: throw new NotImplementedException($"{nameof(AnimRigController)}, {nameof(TrackObj)}: RigType.{rig} has no associated case!");
#endif
        }

        TrackObj(rig, target, delay);
    }

    /// <summary>
    /// Track obj preserving current weight.
    /// </summary>
    /// <param name="rig"></param>
    /// <param name="target"></param>
    /// <param name="delay"></param>
    /// <remarks>Does not need to be called in Update.</remarks>
    public void TrackObj(RigType rig, Transform target, float delay = 0.3f)
    {
        switch (rig)
        {
            case RigType.Head:
                _lookSourcePosLastFrame = lookSource.position;
                lookTrackedObj = target;
                lookTrackingDelay = delay;
                isLookTracking = true;
                break;

            case RigType.ArmLeft:
                isTrackingLeftArm = true;
                _leftArmSourcePosLastFrame = leftArmSource.position;
                leftArmTrackedObj = target;
                leftArmTrackDelay = delay;
                break;

            case RigType.ArmRight:
                isTrackingRightArm = true;
                _rightArmSourcePosLastFrame = rightArmSource.position;
                rightArmTrackedObj = target;
                rightArmTrackDelay = delay;
                break;
            case RigType.All:
                TrackObj(RigType.Head, target, delay);
                TrackObj(RigType.ArmLeft, target, delay);
                TrackObj(RigType.ArmRight, target, delay);
                break;
#if UNITY_EDITOR
            default: throw new NotImplementedException($"{nameof(AnimRigController)}, {nameof(TrackObj)}: RigType.{rig} has no associated case!");
#endif
        }
    }

    /// <summary>
    /// Stops tracking current object and resets tracking and weight values.
    /// </summary>
    public void UntrackObj(RigType rig)
    {
        switch (rig)
        {
            case RigType.Head:
                if (lookWeight != 0)
                {
                    var currentWeight = lookWeight;
                    StartCoroutine(SetWeight(currentWeight, 0f, 0.2f, result => lookWeight = result));
                }

                isLookTracking = false;
                lookTrackedObj = null;
                lookSource.position = _origLookSourcePos;
                _lookSourcePosLastFrame = Vector3.zero;
                break;

            case RigType.ArmLeft:
                if (leftArmWeight != 0)
                {
                    var currentWeight = leftArmWeight;
                    StartCoroutine(SetWeight(currentWeight, 0f, 0.2f, res => leftArmWeight = res));
                }

                isTrackingLeftArm = false;
                leftArmTrackedObj = null;
                leftArmSource.position = _origLeftArmSourcePos;
                _leftArmSourcePosLastFrame = Vector3.zero;
                break;

            case RigType.ArmRight:
                if (rightArmWeight != 0)
                {
                    var currentWeight = rightArmWeight;
                    StartCoroutine(SetWeight(currentWeight, 0f, 0.2f, res => rightArmWeight = res));
                }

                isTrackingRightArm = false;
                rightArmTrackedObj = null;
                rightArmSource.position = _origRightArmSourcePos;
                _rightArmSourcePosLastFrame = Vector3.zero;
                break;

            case RigType.All:
                UntrackObj(RigType.Head);
                UntrackObj(RigType.ArmLeft);
                UntrackObj(RigType.ArmRight);
                break;
#if UNITY_EDITOR
            default: throw new NotImplementedException($"{nameof(AnimRigController)}, {nameof(UntrackObj)}: RigType.{rig} has no associated case!");
#endif
        }
    }

    /// <summary>
    /// Overrides the target object for the specified rig and smoothly sets new weight
    /// and transitions to the new position over the specified delay time.
    /// </summary>
    /// <param name="rig">Rig to override.</param>
    /// <param name="newTarget">Transform of the new target object.</param>
    /// <param name="weight">New rig weight.</param>
    /// <param name="delay">Time in seconds it takes to complete transition.</param>
    public void TargetOverride(RigType rig, Transform newTarget, float weight, float delay = 0.4f)
    {
        switch (rig) 
        {
            case RigType.Head:
                if (lookWeight != weight)
                {
                    var startWeight = lookWeight;
                    StartCoroutine(SetWeight(startWeight, weight, 0.2f, res => lookWeight = res));
                }
                break;
            case RigType.ArmRight:
                if (rightArmWeight != weight)
                {
                    var startWeight = rightArmWeight;
                    StartCoroutine(SetWeight(startWeight, weight, 0.2f, res => rightArmWeight = res));
                }
                break;
            case RigType.ArmLeft:
                if (leftArmWeight != weight)
                {
                    var startWeight = leftArmWeight;
                    StartCoroutine(SetWeight(startWeight, weight, 0.2f, res => leftArmWeight = res));
                }
                break;
            case RigType.All:
                TargetOverride(RigType.Head, newTarget, weight, delay);
                TargetOverride(RigType.ArmLeft, newTarget, weight, delay);
                TargetOverride(RigType.ArmRight, newTarget, weight, delay);
                break;
#if UNITY_EDITOR
            default: throw new NotImplementedException($"{nameof(AnimRigController)}, {nameof(TargetOverride)}: RigType.{rig} has no associated case!");
#endif
        }

        TargetOverride(rig, newTarget, delay);
    }

    /// <summary>
    /// Overrides the target object for the specified rig and smoothly transitions to 
    /// the new target's position over the specified delay time.
    /// </summary>
    /// <param name="rig">Rig to override.</param>
    /// <param name="newTarget">Transform of the new target object.</param>
    /// <param name="delay">Time in seconds it takes to complete transition.</param>
    /// <remarks>Preserves current weight.</remarks>
    public void TargetOverride(RigType rig, Transform newTarget, float delay = 0.4f)
    {
        switch (rig) 
        {
            case RigType.Head:
                lookTrackedObj = newTarget;
                var startPos = lookSource.position;
                StartCoroutine(MoveSource(startPos, lookTrackedObj.position, delay, res => lookSource.position = res));
                break;

            case RigType.ArmRight:
                rightArmTrackedObj = newTarget;
                startPos = rightArmSource.position;
                StartCoroutine(MoveSource(startPos, rightArmTrackedObj.position, delay, res => rightArmSource.position = res));
                break;

            case RigType.ArmLeft:
                leftArmTrackedObj = newTarget;
                startPos = leftArmSource.position;
                StartCoroutine(MoveSource(startPos, leftArmTrackedObj.position, delay, res => leftArmSource.position = res));
                break;

            case RigType.All:
                TargetOverride(RigType.Head, newTarget, delay);
                TargetOverride(RigType.ArmRight, newTarget, delay);
                TargetOverride(RigType.ArmLeft, newTarget, delay);
                break;
#if UNITY_EDITOR
            default: throw new NotImplementedException($"{nameof(AnimRigController)}, {nameof(TargetOverride)}: RigType.{rig} has no associated case!");
#endif
        }
    }

    /// <summary>
    /// Resets all rig overrides to their default values, and resets source position.
    /// </summary>
    public void DiscardAllOverrides(float weightResetTime)
    {
        SetWeight(RigType.All, 0f, weightResetTime);

        isLookTracking = false;
        isTrackingRightArm = false;
        isTrackingLeftArm = false;

        lookTrackedObj = null;
        leftArmTrackedObj = null;
        rightArmTrackedObj = null;

        lookSource.localPosition = _origLookSourcePos;
        leftArmSource.localPosition = _origLeftArmSourcePos;
        rightArmSource.localPosition = _origRightArmSourcePos;
    }
    #endregion

    #region Tracking
    /// <summary>
    /// Tracks the position of the currently tracked object, moving the look source with a delay if the object's position changes.
    /// </summary>
    private void UpdateLookTracking()
    {
        if (isLookTracking)
        {
            Vector3 offset = lookTrackedObj.position - _lookSourcePosLastFrame;

            if (offset.magnitude > lookReactionThreshold)
            {
                _lookSourcePosLastFrame = lookSource.position;

                StartCoroutine(MoveSource(_lookSourcePosLastFrame, lookTrackedObj.position, lookTrackingDelay, result => lookSource.position = result));
            }
        }
    }

    /// <summary>
    /// Tracks the position of the currently tracked object for the Left arm, moving the arm source with a delay if the object's position changes.
    /// </summary>
    private void UpdateLeftArmTracking()
    {
        if (isTrackingLeftArm)
        {
            Vector3 offset = leftArmTrackedObj.position - _leftArmSourcePosLastFrame;
            if (offset.magnitude > leftArmReacitonThreshold)
            {
                _leftArmSourcePosLastFrame = leftArmSource.position;
                StartCoroutine(MoveSource(_leftArmSourcePosLastFrame, leftArmTrackedObj.position, leftArmTrackDelay, res => leftArmSource.position = res));
            }
        }
    }

    /// <summary>
    /// Tracks the position of the currently tracked object for the right arm, moving the arm source with a delay if the object's position changes.
    /// </summary>
    private void UpdateRightArmTracking()
    {
        if (isTrackingRightArm)
        {
            Vector3 offset = rightArmTrackedObj.position - _rightArmSourcePosLastFrame;
            if (offset.magnitude > rightArmReacitonThreshold)
            {
                _rightArmSourcePosLastFrame = rightArmSource.position;
                StartCoroutine(MoveSource(_rightArmSourcePosLastFrame, rightArmTrackedObj.position, rightArmTrackDelay, res => rightArmSource.position = res));
            }
        }
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Interpolates between the original weight and a new weight over a specified time period.
    /// </summary>
    /// <param name="startWeight">Starting weight value.</param>
    /// <param name="newWeight">Target weight value. (Clamped between 0 - 1)</param>
    /// <param name="time">Period over which to interpolate the weight value.</param>
    /// <param name="result">Delegate that handles the interpolated weight values.</param>
    private static IEnumerator SetWeight(float startWeight, float newWeight, float time, Action<float> result)
    {
        newWeight = Mathf.Clamp01(newWeight);

        float timeElapsed = 0f;

        while (timeElapsed < time) 
        {
            result(Mathf.Lerp(startWeight, newWeight, timeElapsed / time));

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        result(newWeight);
    }

    /// <summary>
    /// Moves source object smoothly from its original position to a target position over a given amount of time.
    /// </summary>
    /// <param name="startPos">The object's starting position.</param>
    /// <param name="targetPos">The object's target position.</param>
    /// <param name="time">Time in seconds it should take to complete the movement.</param>
    /// <param name="result">Delegate that handles the interpolated position</param>
    private static IEnumerator MoveSource(Vector3 startPos, Vector3 targetPos, float time, Action<Vector3> result)
    {
        float timeElapsed = 0f;

        while (timeElapsed < time)
        {
            result(Vector3.Lerp(startPos, targetPos, timeElapsed / time));

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        result(targetPos);
    }
    #endregion
}
