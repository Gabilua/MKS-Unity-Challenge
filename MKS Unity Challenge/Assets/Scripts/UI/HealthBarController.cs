using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] Image _healthBar;
    HealthManager _healthManager;
    Camera _camera;

    public void Setup(HealthManager health)
    {
        _healthManager = health;
        _camera = UIManager.uiManagerInstance._camera;
    }

    private void Update()
    {
        if (!_healthManager.gameObject.activeInHierarchy || _healthManager.IsDead || _healthManager == null)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);

        _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _healthManager._currentHealth / _healthManager._maxHealth, 10 * Time.deltaTime);
        transform.position = _camera.WorldToScreenPoint(_healthManager.transform.position);
    }
}
