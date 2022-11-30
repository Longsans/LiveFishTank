using UnityEngine;
using UnityEngine.UI;

public class ScaleSliders : MonoBehaviour
{
    [SerializeField]
    private Slider widthSlider,
                    lengthSlider,
                    heightSlider;

    // Start is called before the first frame update
    void Start()
    {
        HandleCurrentSelectedPlaceableChanged(null);
        InteractionManager
            .Instance
            .CurrentSelectedPlaceableChanged
            .AddListener(HandleCurrentSelectedPlaceableChanged);
    }

    /// <summary>
    /// Determine if the sliders for scale modification should be shown for the <c>GameObject</c> that the player selected.
    /// </summary>
    /// <param name="gameObj"></param>
    private void HandleCurrentSelectedPlaceableChanged(GameObject gameObj)
    {
        if (gameObj == null || !gameObj.TryGetComponent<FishTank>(out var fishTank))
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        widthSlider.onValueChanged.RemoveAllListeners();
        lengthSlider.onValueChanged.RemoveAllListeners();
        heightSlider.onValueChanged.RemoveAllListeners();

        widthSlider.value = fishTank.transform.localScale.x;
        lengthSlider.value = fishTank.transform.localScale.z;
        heightSlider.value = fishTank.transform.localScale.y;

        widthSlider.onValueChanged.AddListener(w => fishTank.SetTankWidth(w));
        lengthSlider.onValueChanged.AddListener(l => fishTank.SetTankLength(l));
        heightSlider.onValueChanged.AddListener(h => fishTank.SetTankHeight(h));
    }
}
