using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Dev_Trainer : MonoBehaviour
{
    [SerializeField] PlayerFSMController controller;
    [SerializeField] AnimRigController animRigController;
    [SerializeField] bool toggalable = true;
    [Space]
    [SerializeField] GameObject trainerUi;
    [Space]
    [SerializeField] Button reloadSceneBtn;
    [SerializeField] Button toggleRagdollBtn;
    [SerializeField] Button recoverBtn;
    [SerializeField] Button resetAnimOverridesBtn;
    [SerializeField] Button setLookBtn;

    // Start is called before the first frame update
    void Start()
    {
        reloadSceneBtn.onClick.AddListener(OnReloadScene);
        toggleRagdollBtn.onClick.AddListener(OnRagdollBtnDown);
        recoverBtn.onClick.AddListener(OnRecoverBtnDown);
        resetAnimOverridesBtn.onClick.AddListener(OnResetAnimsDown);

        setLookBtn.onClick.AddListener(OnSetLookDown);
    }

    void OnReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnRagdollBtnDown()
    {
        if (!controller.StateManager.CompareCurrentState(PlayerState.Controllable)) return;

        controller.AddForce(200f);
    }

    private void OnRecoverBtnDown()
    {
        if (!controller.StateManager.CompareCurrentState(PlayerState.Ragdoll)) return;

        controller.StateManager.SetState(PlayerState.Recovering);
    }

    private void OnResetAnimsDown()
    {
        animRigController.DiscardAllOverrides(animRigController.TEST_resetTime);
    }

    private void OnSetLookDown()
    {
        animRigController.TargetOverride(AnimRigController.RigType.Head, animRigController.testTarget, weight:0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (toggalable)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleTrainer();
            }
        }
        else
        {
            trainerUi.SetActive(true);
        }
    }

    void ToggleTrainer()
    {
        trainerUi.SetActive(!trainerUi.activeSelf);

        if (trainerUi.activeSelf) 
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
