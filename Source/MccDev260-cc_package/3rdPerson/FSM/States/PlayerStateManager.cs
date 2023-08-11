using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerState
{
    /// <summary>
    /// Can be controlled by player.
    /// </summary>
    Controllable,
    /// <summary>
    /// Not accepting player input.
    /// </summary>
    Wild,
    /// <summary>
    /// Physics system, take the wheel.
    /// </summary>
    Ragdoll,
    /// <summary>
    /// Need a breather;
    /// </summary>
    Recovering,
}

public class PlayerStateManager
{
    static Dictionary<PlayerState, IPlayerState> stateDict;

    IPlayerState _currentState;
    public IPlayerState CurrentState { get { return _currentState; } }

    public PlayerStateManager(PlayerFSMController controller)
    {
        stateDict = new Dictionary<PlayerState, IPlayerState>()
        {
            { PlayerState.Controllable, new ControlableState(controller) },
            { PlayerState.Ragdoll, new RagdollState(controller) },
            { PlayerState.Recovering, new RecoveringState(controller) },
        };

        foreach (IPlayerState state in stateDict.Values)
        {
            state.OnLoad();
        }
    }

    public void SetState(PlayerState s)
    {
        var newState = stateDict[s];
        if (_currentState != null)
        {
            if (_currentState == newState) return;

            _currentState.OnExit();
        }
        _currentState = newState;
        _currentState.OnEnter();

#if UNITY_EDITOR
        Debug.Log($"Player State set to {newState}");
#endif
    }

    public bool CompareCurrentState(PlayerState s)
    {
        if (_currentState != null)
        {
            if (stateDict[s] == _currentState) return true;
        }

        return false;
    }

    public void OnDestroy()
    {
        Debug.Log("Destroy da ting");
        foreach (IPlayerState state in stateDict.Values)
        {
            state.OnDestroy();
        }
    }
}
