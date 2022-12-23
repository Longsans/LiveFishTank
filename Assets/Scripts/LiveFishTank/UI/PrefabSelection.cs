using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabSelection : MonoBehaviour
{
    [SerializeField]
    private GameObject _previousModel,
                        _currentModel,
                        _nextModel;
    [SerializeField]
    private Button _previousButton, _currentButton, _nextButton;

    [SerializeField]
    private List<GameObject> _fishTypes,
                            _foodTypes;
    [SerializeField]
    private float _middleModelScale, _sideModelsScale;

    // Start is called before the first frame update
    void Start()
    {
        PlaceablesManager.Instance.FinishedPlacingTank
            .AddListener(OnFinishedPlacingTank);

        _previousButton.onClick.AddListener(SelectPrevious);
        _nextButton.onClick.AddListener(SelectNext);
        _currentButton.interactable = false;
    }

    public void SelectPrevious()
    {
        GameObject nextModel;
        GameObject currentModel;
        GameObject previousModel;
        if (PlaceablesManager.Instance.ResidentType == TankResidentType.Fish)
        {
            var previousIndex = PlaceablesManager.Instance.PreviousSeveralFishIndex(1);
            nextModel = _fishTypes[PlaceablesManager.Instance.SelectedFishPrefabIndex];
            currentModel = _fishTypes[previousIndex];
            previousModel = _fishTypes[PlaceablesManager.Instance.PreviousSeveralFishIndex(2)];
            PlaceablesManager.Instance.SelectedFishPrefabIndex = previousIndex;
            ChangeModels(previousModel, currentModel, nextModel);
        }
        Debug.Log($"prefab index: {PlaceablesManager.Instance.SelectedFishPrefabIndex}");
    }

    public void SelectNext()
    {
        GameObject previousModel;
        GameObject currentModel;
        GameObject nextModel;
        if (PlaceablesManager.Instance.ResidentType == TankResidentType.Fish)
        {
            var nextIndex = PlaceablesManager.Instance.NextSeveralFishIndex(1);
            previousModel = _fishTypes[PlaceablesManager.Instance.SelectedFishPrefabIndex];
            currentModel = _fishTypes[nextIndex];
            nextModel = _fishTypes[PlaceablesManager.Instance.NextSeveralFishIndex(2)];
            PlaceablesManager.Instance.SelectedFishPrefabIndex = nextIndex;
            ChangeModels(previousModel, currentModel, nextModel);
        }
        Debug.Log($"prefab index: {PlaceablesManager.Instance.SelectedFishPrefabIndex}");
    }

    private void ChangeModels(GameObject previous, GameObject current, GameObject next)
    {
        var oldPrev = _previousModel;
        var oldCurrent = _currentModel;
        var oldNext = _nextModel;
        _previousModel = Instantiate(previous, Vector3.zero, Quaternion.identity, _previousButton.transform);
        _currentModel = Instantiate(current, Vector3.zero, Quaternion.identity, _currentButton.transform);
        _nextModel = Instantiate(next, Vector3.zero, Quaternion.identity, _nextButton.transform);

        _previousModel.transform.localPosition =
            _currentModel.transform.localPosition =
                _nextModel.transform.localPosition = Vector3.zero;

        _currentModel.transform.localScale *= _middleModelScale;
        _previousModel.transform.localScale *= _sideModelsScale;
        _nextModel.transform.localScale *= _sideModelsScale;
        Destroy(oldPrev);
        Destroy(oldCurrent);
        Destroy(oldNext);
    }

    private void OnFinishedPlacingTank()
    {
        gameObject.SetActive(PlaceablesManager.Instance.ResidentType == TankResidentType.Fish);
    }
}
