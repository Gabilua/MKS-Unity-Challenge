using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsSlider : MonoBehaviour
{
    public delegate void ValueUpdated(float value);
    public event ValueUpdated OnValueUpdated;

    Slider _slider;
    [SerializeField] TextMeshProUGUI _valueDisplay;
    
    public void UpdateValue()
    {
        if (_slider == null)
            _slider = GetComponentInChildren<Slider>();

        _valueDisplay.text = _slider.value.ToString("F0");
        OnValueUpdated?.Invoke(_slider.value);
    }
}
