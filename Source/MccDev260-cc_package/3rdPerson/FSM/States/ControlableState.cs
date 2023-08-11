using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class ControlableState : IPlayerState
{
    PlayerFSMController IPlayerState.Controller { get; set; }

    #region State Data
    // Movement
    private PlayerFSMController _controller;
    private GameObject _mainCamera;
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;


    // Gravity / Jumping
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // Anim IDs
    private int _animIDSpeed;
    private int _animIDMotionSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    #endregion 

    public ControlableState(PlayerFSMController controller)
    {
        _controller = controller;
    }

    void IPlayerState.OnLoad()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        AssignAnimationIDs();
    }

    void IPlayerState.OnEnter()
    {
    }

    void IPlayerState.OnExit()
    {
        _animationBlend = 0;
        _speed = 0;
        _rotationVelocity = 0;
    }

    void IPlayerState.OnUpdate()
    {   
        JumpAndGravity();
        Move();
        GroundedCheck();
    }

    void IPlayerState.OnDestroy()
    {
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _controller.gameInputMap.Sprint ? _controller.SprintSpeed : _controller.MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_controller.gameInputMap.Move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.AttachedController.velocity.x, 0.0f, _controller.AttachedController.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _controller.inputManager.analogMovement ? _controller.gameInputMap.Move.magnitude : 1f;


        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * _controller.SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * _controller.SpeedChangeRate);

        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_controller.gameInputMap.Move.x, 0.0f, _controller.gameInputMap.Move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_controller.gameInputMap.Move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(_controller.PlayerObject.transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _controller.RotationSmoothTime);

            // rotate to face input direction relative to camera position
            _controller.PlayerObject.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        _controller.AttachedController.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        _controller.Animator.SetFloat(_animIDSpeed, _animationBlend);
        _controller.Animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
    }

    private void JumpAndGravity()
    {
        if (_controller.Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = _controller.FallTimeout;

            _controller.Animator.SetBool(_animIDJump, false);
            _controller.Animator.SetBool(_animIDFreeFall, false);

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_controller.gameInputMap.Jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(_controller.JumpHeight * -2f * _controller.Gravity);

                // update animator if using character
                _controller.Animator.SetBool(_animIDJump, true);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = _controller.JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                _controller.Animator.SetBool(_animIDFreeFall, true);
            }

            // if we are not grounded, do not jump
            _controller.gameInputMap.SetJumpInput(false);
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += _controller.Gravity * Time.deltaTime;
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(_controller.PlayerObject.transform.position.x, _controller.PlayerObject.transform.position.y - _controller.GroundedOffset,
            _controller.PlayerObject.transform.position.z);
        _controller.Grounded = Physics.CheckSphere(spherePosition, _controller.GroundedRadius, _controller.GroundLayers,
            QueryTriggerInteraction.Ignore);

        _controller.Animator.SetBool(_animIDGrounded, _controller.Grounded);
    }
}
