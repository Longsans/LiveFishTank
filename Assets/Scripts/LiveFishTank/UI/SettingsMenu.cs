using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private string _showVarName = "Show";

    void Start()
    {
        HandleSettingsMenuFinishedHide();
        InteractionManager
            .Instance
            .SettingsMenuFinishedHide
            .AddListener(HandleSettingsMenuFinishedHide);
    }

    public void ToggleSettingsMenuOpen(bool open)
    {
        if (open)
            gameObject.SetActive(open);

        _animator.SetBool(_showVarName, open);
    }

    private void HandleSettingsMenuFinishedHide()
    {
        gameObject.SetActive(false);
    }
}
