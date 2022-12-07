using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Toggles : MonoBehaviour
{
    [SerializeField] private Toggle _interactionModeToggle;
    [SerializeField] private Toggle _objectModeToggle;
    [SerializeField] private Toggle _visualizeTanksToggle;
    [SerializeField] private Image _objectModeTankImage;
    [SerializeField] private Image _objectModeFishImage;
    [SerializeField] private TMP_Text _objectModeDisplayText;

    // Start is called before the first frame update
    void Start()
    {
        HandleObjectModeToggle(_objectModeToggle);
        HandleVisualizeTankToggle(_visualizeTanksToggle);
        HandleModifyOnChanged();
        InteractionManager.Instance.ModifyOnChanged.AddListener(HandleModifyOnChanged);
    }

    public void HandleVisualizeTankToggle(Toggle visualizeTankToggle)
    {
        PlaceablesManager.Instance.ShowGeospatialObjectsBounds = visualizeTankToggle.isOn;
    }

    public void HandleInteractionModeToggle(Toggle toggle)
    {
        if (InteractionManager.Instance.ModifyOn != toggle.isOn)
            InteractionManager.Instance.SetModifyInvoke(toggle.isOn);
    }

    public void HandleObjectModeToggle(Toggle toggle)
    {
        _objectModeTankImage.enabled = toggle.isOn;
        _objectModeFishImage.enabled = !toggle.isOn;
        if (toggle.isOn)
        {
            InteractionManager.Instance.ObjectMode = PlaceableType.Tank;
            _objectModeDisplayText.text = "Tank";
        }
        else
        {
            InteractionManager.Instance.ObjectMode = PlaceableType.Fish;
            _objectModeDisplayText.text = "Fish";
        }
    }

    private void HandleModifyOnChanged()
    {
        _interactionModeToggle.isOn = InteractionManager.Instance.ModifyOn;
    }
}
