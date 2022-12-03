using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectModeToggle : MonoBehaviour
{
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private Image _tankImage;
    [SerializeField] private Image _fishImage;

    // Start is called before the first frame update
    void Start()
    {
        var toggle = GetComponent<Toggle>();
        HandleToggleValueChanged(toggle);
    }

    public void HandleToggleValueChanged(Toggle toggle)
    {
        _tankImage.enabled = toggle.isOn;
        _fishImage.enabled = !toggle.isOn;
        if (toggle.isOn)
        {
            InteractionManager.Instance.ObjectMode = PlaceableType.Tank;
            _displayText.text = "Tank";
        }
        else
        {
            InteractionManager.Instance.ObjectMode = PlaceableType.Fish;
            _displayText.text = "Fish";
        }
    }
}
