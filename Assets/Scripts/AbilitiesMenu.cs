using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Abilities
{
    None,
    Ascend,
    Camera,
    Ultrahand,
    Autobuild,
    Fuse,
    Recall,
    Amiibo,
    Map,
}

public class AbilitiesMenu : SingletonPattern<AbilitiesMenu>
{
    public GameObject abilitiesCanvas;
    public GameObject ascendControlsPanel;
    public GameObject ascendExitPanel;
    public Image abilityImage;
    public Sprite ascendIcon;
    public Sprite noneIcon;
    private Ascend ascendAbility;

    private void Start()
    {
        ascendAbility = GetComponent<Ascend>();
        ascendControlsPanel.SetActive(false);
        ascendExitPanel.SetActive(false);
    }

    public void ActivateAscend()
    {
        if (!ascendAbility.ascendMode)
        {
            CloseAbilitesMenu();
            ascendControlsPanel.SetActive(true);
            abilityImage.sprite = ascendIcon;
            PlayerController.Instance.CurrAbility = Abilities.Ascend;
            CameraController.Instance.ActivateAscendCam();
            ascendAbility.ascendMode = true;
        }
    }

    public void ActivateOther()
    {
        DectivateAbility();

        abilityImage.sprite = noneIcon;
        PlayerController.Instance.CurrAbility = Abilities.None;
        CloseAbilitesMenu();
    }


    public void DectivateAbility()
    {
        if (ascendAbility.ascendMode)
        {
            ascendAbility.ascendMode = false;
            ascendControlsPanel.SetActive(false);
            ascendAbility.gridProjector.enabled = false;
            CameraController.Instance.ActivateMainCam();
        }        
    }

    public void OpenAbilitesMenu()
    {
        Cursor.lockState = CursorLockMode.Confined;
        abilitiesCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseAbilitesMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        abilitiesCanvas.SetActive(false);
        Time.timeScale = 1f;
    }
}
