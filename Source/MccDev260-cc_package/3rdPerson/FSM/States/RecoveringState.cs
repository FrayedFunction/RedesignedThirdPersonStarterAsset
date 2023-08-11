using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoveringState : IPlayerState
{
    PlayerFSMController IPlayerState.Controller { get; set; }

    PlayerFSMController _controller;
    public RecoveringState(PlayerFSMController controller)
    {
        _controller = controller;
    }

    void IPlayerState.OnEnter()
    {
    }

    void IPlayerState.OnExit()
    {
    }

    void IPlayerState.OnLoad()
    {
    }

    void IPlayerState.OnUpdate()
    {
        if (AnimFinished())
        {
            Debug.Log("Anim Is Fin");
            _controller.StateManager.SetState(PlayerState.Controllable);
        }
    }

    void IPlayerState.OnDestroy() 
    { 

    }
    
    bool AnimFinished()
    {
        return _controller.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
    } 
}
