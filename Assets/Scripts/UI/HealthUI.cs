using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Color Settings")]
    [SerializeField] private Color _colorAtFullHealth;
    [SerializeField] private Gradient _healthGradient;
    [SerializeField] private Color _healthBackgroundColor;

    [Header("Health Bar UI Components")]
    [SerializeField] Slider _slider;
    [SerializeField] Image _healthForeground;
    [SerializeField] Image _healthBackground;

    [Header("Player Health")]
    [SerializeField] PlayerHealth _playerHealth;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.maxValue = _playerHealth.maxHealth;
    }

    public void _updateWithHealth(float newHealth)
    {
        _slider.value = newHealth;

        if (_slider.value <= 0)
        {
            Debug.Log("ui = 0");
            //_healthForeground.gameObject.SetActive(false);
        }
        else
        {
            _healthForeground.gameObject.SetActive(true);

            if (_slider.value >= _slider.maxValue)
            {
                Debug.Log(_colorAtFullHealth);
                _colorAtFullHealth.a = 1f;
                _healthForeground.color = _colorAtFullHealth;
            Debug.Log("2");

            }
            else
            {
                _healthForeground.color = _healthGradient.Evaluate((float)_slider.value / _slider.maxValue);
                Debug.Log("3");

            }
        }
    }
}
