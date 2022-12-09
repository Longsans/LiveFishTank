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
    [SerializeField] private Image _objectModeFeedingImage;
    private Image[] _objectModeIcons;
    private int _currentModeIcon = -1;

    // Start is called before the first frame update
    void Start()
    {
        _objectModeIcons = new Image[] { _objectModeFishImage, _objectModeTankImage, _objectModeFeedingImage };
        HandleObjectModeToggle();
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

    public void HandleObjectModeToggle()
    {
        _currentModeIcon = (_currentModeIcon + 1) % _objectModeIcons.Length;
        for (int i = 0; i < _objectModeIcons.Length; i++)
        {
            _objectModeIcons[i].enabled = i == _currentModeIcon;
        }
        if (_objectModeFishImage.enabled)
            InteractionManager.Instance.ObjectMode = PlaceableType.Fish;
        else if (_objectModeTankImage.enabled)
            InteractionManager.Instance.ObjectMode = PlaceableType.Tank;
        else if (_objectModeFeedingImage.enabled)
            InteractionManager.Instance.ObjectMode = PlaceableType.FishFood;
    }

    private void HandleModifyOnChanged()
    {
        _interactionModeToggle.isOn = InteractionManager.Instance.ModifyOn;
    }
}
