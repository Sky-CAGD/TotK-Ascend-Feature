using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesMenu : MonoBehaviour
{
    public GameObject abilitiesCanvas;
    private Ascend ascendAbility;

    private void Start()
    {
        ascendAbility = GetComponent<Ascend>();
    }

    public void ActivateAscend()
    {
        if(!ascendAbility.ascendMode)
        {
            CameraController.Instance.ActivateAscendCam();
            ascendAbility.ascendMode = true;
        }
    }

    public void DectivateAbility()
    {
        if (ascendAbility.ascendMode)
        {
            ascendAbility.ascendMode = false;
            ascendAbility.gridProjector.enabled = false;
            CameraController.Instance.ActivateMainCam();
        }        
    }
}
