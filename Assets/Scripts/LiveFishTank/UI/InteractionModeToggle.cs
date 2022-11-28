using UnityEngine;
using UnityEngine.UI;

public class InteractionModeToggle : MonoBehaviour
{
    private Toggle _toggle;

    public void HandleToggleValueChanged(Toggle toggle)
    {
        if (InteractionManager.Instance.ModifyOn != toggle.isOn)
            InteractionManager.Instance.SetModifyInvoke(toggle.isOn);
    }

    private void Start()
    {
        _toggle = GetComponent<Toggle>();
        HandleModifyOnChanged();
        InteractionManager.Instance.ModifyOnChanged.AddListener(HandleModifyOnChanged);
    }

    private void HandleModifyOnChanged()
    {
        _toggle.isOn = InteractionManager.Instance.ModifyOn;
    }
}
