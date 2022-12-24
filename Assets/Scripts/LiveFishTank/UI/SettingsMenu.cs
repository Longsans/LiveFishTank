using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Button _placeTankButton;
    [SerializeField] private TMP_Text _placeTankText;
    [SerializeField] private PrefabSelection _prefabSelectionSection;

    private string _showVarName = "Show";

    void Start()
    {
        OnSettingsMenuFinishedHiding();
        InteractionManager.Instance.SettingsMenuFinishedHiding
            .AddListener(OnSettingsMenuFinishedHiding);
        PlaceablesManager.Instance.FinishedPlacingTank
            .AddListener(OnFinishedPlacingTank);
    }

    public void ToggleSettingsMenuOpen(bool open)
    {
        if (open)
        {
            gameObject.SetActive(open);
            _prefabSelectionSection.gameObject.SetActive(false);
        }

        _animator.SetBool(_showVarName, open);
    }

    private void OnSettingsMenuFinishedHiding()
    {
        gameObject.SetActive(false);
        _prefabSelectionSection.gameObject.SetActive(
            PlaceablesManager.Instance.ResidentType == TankResidentType.Fish && !PlaceablesManager.Instance.PlacingTank);
    }

    public void HandleStartPlacingTank()
    {
        InteractionManager.Instance.SelectPlaceableGameObject(null);
        PlaceablesManager.Instance.StartTankPlacement();
        _placeTankButton.interactable = false;
        ToggleSettingsMenuOpen(false);
    }

    public void OnFinishedPlacingTank()
    {
        _placeTankButton.interactable = true;
        if (PlaceablesManager.Instance.TankPlaced)
            _placeTankText.text = "Move tank";
    }
}
