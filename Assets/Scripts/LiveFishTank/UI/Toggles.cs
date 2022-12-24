using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Toggles : MonoBehaviour
{
    [SerializeField] private Toggle _residentTypeToggle;
    [SerializeField] private Image _residentTypeFishImage;
    [SerializeField] private Image _residentTypeFeedingImage;
    private Image[] _residentTypeIcons;
    private int _currentModeIcon = -1;

    // Start is called before the first frame update
    void Start()
    {
        _residentTypeIcons = new Image[] { _residentTypeFishImage, _residentTypeFeedingImage };
        HandleResidentTypeToggle();
        PlaceablesManager.Instance.StartedPlacingTank
            .AddListener(OnStartedPlacingTank);
        PlaceablesManager.Instance.FinishedPlacingTank
            .AddListener(OnFinishedPlacingTank);
    }

    public void HandleModifyingTankToggle(Toggle toggle)
    {
        if (InteractionManager.Instance.ModifyingTank != toggle.isOn)
            InteractionManager.Instance.SetModifyingTankInvoke(toggle.isOn);
    }

    public void HandleResidentTypeToggle()
    {
        _currentModeIcon = (_currentModeIcon + 1) % _residentTypeIcons.Length;
        for (int i = 0; i < _residentTypeIcons.Length; i++)
        {
            _residentTypeIcons[i].enabled = i == _currentModeIcon;
        }
        if (_residentTypeFishImage.enabled)
            PlaceablesManager.Instance.ResidentType = TankResidentType.Fish;
        else if (_residentTypeFeedingImage.enabled)
            PlaceablesManager.Instance.ResidentType = TankResidentType.FishFood;
    }

    private void OnStartedPlacingTank()
    {
        var halfTransparent = Color.white;
        halfTransparent.a = 170f / 255f;
        _residentTypeIcons[_currentModeIcon].color = halfTransparent;
        _residentTypeToggle.interactable = false;
    }

    private void OnFinishedPlacingTank()
    {
        _residentTypeIcons[_currentModeIcon].color = Color.white;
        _residentTypeToggle.interactable = true;
    }
}
