using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankPlacementFinishButtons : MonoBehaviour
{
    [SerializeField] private Button _okButton;

    void Start()
    {
        PlaceablesManager.Instance.StartedPlacingTank
            .AddListener(OnStartPlacingTank);
        gameObject.SetActive(false);
    }

    void Update()
    {
        _okButton.interactable = PlaceablesManager.Instance.TankPlaceable;
    }

    public void HandleConfirmTankPlacement()
    {
        PlaceablesManager.Instance.ConfirmTankPlacement();
        gameObject.SetActive(false);
    }

    public void HandleCancelTankPlacement()
    {
        PlaceablesManager.Instance.CancelTankPlacement();
        gameObject.SetActive(false);
    }

    private void OnStartPlacingTank()
    {
        gameObject.SetActive(true);
    }
}
