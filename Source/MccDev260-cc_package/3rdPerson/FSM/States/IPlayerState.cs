using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerState
{
    protected PlayerFSMController Controller { get; set; }

    /// <summary>
    /// Called before the first frame.
    /// </summary>
    public void OnLoad();

    /// <summary>
    /// Called on entry to state.
    /// </summary>
    public void OnEnter();

    /// <summary>
    /// Called each frame.
    /// </summary>
    public void OnUpdate();

    /// <summary>
    /// Called on state exit.
    /// </summary>
    public void OnExit();

    /// <summary>
    /// Called when FSM is destroyed.
    /// </summary>
    public void OnDestroy();


}
