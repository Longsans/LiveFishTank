using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsMenuBackdrop : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private SettingsMenu _settingsMenu;

    public void OnPointerDown(PointerEventData eventData)
    {
        _settingsMenu.ToggleSettingsMenuOpen(false);
    }
}
