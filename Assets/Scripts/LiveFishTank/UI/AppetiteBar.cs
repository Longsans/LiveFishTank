using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppetiteBar : MonoBehaviour
{
    [SerializeField] private Fish _fish;
    [SerializeField] private Image _fill;
    [SerializeField] private Image _emote;
    [SerializeField] private Image _emoteBorder;
    [SerializeField] private Color _fishHungryColor;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _fishFullColor;
    [SerializeField] private Color _fishSickColor;
    [SerializeField] private Color _fishDeadColor;
    [SerializeField] private Sprite _hungrySprite;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _fullSprite;
    [SerializeField] private Sprite _sickSprite;
    [SerializeField] private Sprite _deadSprite;
    private Slider _slider;

    // Start is called before the first frame update
    void Start()
    {
        _slider = GetComponent<Slider>();
        _fish.AllDataInitialized.AddListener(HandleFishDataReady);
    }

    // Update is called once per frame
    void Update()
    {
        if (_fish.CurrentSatiety != _slider.value || _fish.MaxSatiety != _slider.maxValue)
        {
            _slider.value = _fish.CurrentSatiety;
            _slider.maxValue = _fish.MaxSatiety;
            if (_slider.value == _slider.maxValue)
            {
                _fill.color = _fishFullColor;
                _emoteBorder.color = _fishFullColor;
                _emote.sprite = _fullSprite;
            }
            else if (_slider.value - _slider.minValue < 0.25f)
            {
                _fill.color = _fishHungryColor;
                _emoteBorder.color = _fishHungryColor;
                _emote.sprite = _hungrySprite;
            }
            else
            {
                _fill.color = _normalColor;
                _emoteBorder.color = _normalColor;
                _emote.sprite = _normalSprite;
            }
        }
    }

    private void HandleFishDataReady()
    {
        _slider.minValue = 0f;
        _slider.maxValue = _fish.MaxSatiety;
        _slider.value = _fish.CurrentSatiety;
    }
}
