using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : SingletonPattern<CameraController>
{
    [SerializeField] private CinemachineStateDrivenCamera csdc;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject linkModel;

    private float transitionTime;

    public bool usingMainCam;

    protected override void Awake()
    {
        base.Awake();
        usingMainCam = true;
        transitionTime = csdc.m_DefaultBlend.m_Time;
    }

    public void ActivateMainCam()
    {
        if (!usingMainCam)
        {
            usingMainCam = true;
            linkModel.SetActive(true);
            animator.Play("MainCamera");
        }
    }

    public void ActivateAscendCam()
    {
        if (usingMainCam)
        {
            usingMainCam = false;
            animator.Play("AscendCamera");
            StartCoroutine(UnrenderLink());
        }
    }

    private IEnumerator UnrenderLink()
    {
        yield return new WaitForSeconds(transitionTime);

        if(!usingMainCam)
            linkModel.SetActive(false);
    }
}
