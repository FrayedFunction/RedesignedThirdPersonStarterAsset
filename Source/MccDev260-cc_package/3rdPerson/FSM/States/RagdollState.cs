using System.Collections.Generic;
using UnityEngine;

public class RagdollState : IPlayerState
{
    PlayerFSMController IPlayerState.Controller { get; set; }

    PlayerFSMController _controller;

    static readonly List<Collider> _ragdollColls = new();
    static readonly List<Rigidbody> _ragdollRbs = new();

    Transform _hipBone;

    #region Lifetime
    public RagdollState(PlayerFSMController controller)
    {
        _controller = controller;
    }

    void IPlayerState.OnLoad()
    {
        // Find all colliders and rigidbodys on skeleton obj: Add to lists.
        Collider[] colls = _controller.SkeletonObj.GetComponentsInChildren<Collider>();
        _ragdollColls.AddRange(colls);

        Rigidbody[] rbs = _controller.SkeletonObj.GetComponentsInChildren<Rigidbody>();
        _ragdollRbs.AddRange(rbs);
        
        _hipBone = _controller.Animator.GetBoneTransform(HumanBodyBones.Hips);

        ToggleCollidersAsTriggers(true);
        ToggleKinimaticRbs(true);
    }
    void IPlayerState.OnEnter()
    {
        _controller.RigController.DiscardAllOverrides(_controller.RigController.TEST_resetTime);

        _controller.Animator.enabled = false;
        _controller.AttachedController.enabled = false;

        ToggleCollidersAsTriggers(false);
        ToggleKinimaticRbs(false);
    }
    void IPlayerState.OnUpdate()
    {
    }
    void IPlayerState.OnExit()
    {
        SetPostionToHips();

        ToggleCollidersAsTriggers(true);
        ToggleKinimaticRbs(true);

        _controller.Animator.enabled = true;
        _controller.AttachedController.enabled = true;

        if (_controller.ObjOrientation.CompareCurrentDirection(ObjOrientation.Direction.Down))
        {
            _controller.Animator.Play("StandUp-Front", -1, 0);
        }
        else
        {
            _controller.Animator.Play("StandUp-Back", -1, 0);
        }
    }
    void IPlayerState.OnDestroy()
    {
        _ragdollRbs.Clear();
        _ragdollColls.Clear();
    }
    #endregion

    #region Physics
    public void AddForce(Vector3 force)
    {
        foreach (Rigidbody rb in _ragdollRbs) 
        { 
            rb.AddForce(force);
        }
    }
    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        foreach (Rigidbody rb in _ragdollRbs)
        {
            rb.AddForce(force, forceMode);
        }
    }
    
    public void AddForceLimb(HumanBodyBones bone, Vector3 force)
    {
        var gameObj = _controller.Animator.GetBoneTransform(bone).gameObject;
        var rb = gameObj.GetComponent<Rigidbody>();
        var col = gameObj.GetComponent<Collider>();


        if (rb.isKinematic && col.isTrigger)
        {
            rb.isKinematic = false;
            col.isTrigger = false;
        }

        rb.AddForce(force);
    }

    public void AddExplosionForce(float force, Vector3 explosionPosition, float explosionRadius)
    {
        foreach (Rigidbody rb in _ragdollRbs)
        {
            rb.AddExplosionForce(force, explosionPosition, explosionRadius);
        }
    }
    public void AddExplosionForce(float force, Vector3 explosionPosition, float explosionRadius, float upwardsModifier)
    {
        foreach (Rigidbody rb in _ragdollRbs)
        {
            rb.AddExplosionForce(force, explosionPosition, explosionRadius,upwardsModifier);
        }
    }
    public void AddExplosionForce(float force, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode forceMode)
    {
        foreach (Rigidbody rb in _ragdollRbs)
        {
            rb.AddExplosionForce(force, explosionPosition, explosionRadius, upwardsModifier, forceMode);
        }
    }


    #endregion

    private void ToggleCollidersAsTriggers(bool v)
    {
        foreach (Collider c in _ragdollColls)
        {
            c.isTrigger = v;
        }
    }

    private void ToggleKinimaticRbs(bool v)
    {
        foreach (Rigidbody rb in _ragdollRbs)
        {
            rb.isKinematic = v;
        }
    }

    private void SetPostionToHips()
    {
        Vector3 ogHipPos = _hipBone.position;
        _controller.PlayerObject.transform.position = _hipBone.position;
  
        if (Physics.Raycast(_controller.PlayerObject.transform.position, Vector3.down, out RaycastHit hit, 2, LayerMask.GetMask("Ground")))
        {
            _controller.PlayerObject.transform.position = new Vector3(_controller.PlayerObject.transform.position.x, hit.point.y, _controller.PlayerObject.transform.position.z);
        }

        _hipBone.position = ogHipPos;
    }
}
