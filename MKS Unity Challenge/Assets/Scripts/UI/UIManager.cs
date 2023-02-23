using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager uiManagerInstance;

    public Camera _camera;

    [SerializeField] GameObject _healthBarPrefab;
    [SerializeField] Transform _healthBarHolder;

    [SerializeField] SettingsSlider _matchDurationSlider;
    [SerializeField] SettingsSlider _enemySpawnIntervalSlider;

    [SerializeField] GameObject _hud;
    [SerializeField] GameObject _menus;
    [SerializeField] GameObject _mainMenu;
    [SerializeField] GameObject _settingsScreen;
    [SerializeField] GameObject _resultsScreen;

    [SerializeField] TextMeshProUGUI _playerKillsDisplay;

    [SerializeField] TextMeshProUGUI _matchTimer;

    List<HealthBarController> _healthBarPool = new List<HealthBarController>();

    private void Awake()
    {
        if (uiManagerInstance == null)
            uiManagerInstance = this;
    }

    private void Start()
    {
        _matchDurationSlider.OnValueUpdated += UpdateMatchDuration;
        _enemySpawnIntervalSlider.OnValueUpdated += UpdateEnemySpawnInterval;

        ResetUI();
    }

    public void NewHealthBar(HealthManager owner)
    {
        HealthBarController newHealthBar = AvailableHealthBarInPool();
        newHealthBar.gameObject.SetActive(true);

        newHealthBar.Setup(owner);
    }

    HealthBarController AvailableHealthBarInPool()
    {
        HealthBarController availableHealthBar = null;

        foreach (var healthBar in _healthBarPool)
        {
            if (!healthBar.gameObject.activeInHierarchy)
            {
                availableHealthBar = healthBar;
                break;
            }
        }

        if (availableHealthBar == null)
        {
            GameObject healthBar = Instantiate(_healthBarPrefab, _healthBarHolder);

            HealthBarController healthBarController = healthBar.GetComponent<HealthBarController>();

            _healthBarPool.Add(healthBarController);
            availableHealthBar = healthBarController;
        }

        return availableHealthBar;
    }

    void UpdateMatchDuration(float seconds)
    {
        GameManager.gameManagerInstance.SetMatchDuration((int)seconds);
    }

    void UpdateEnemySpawnInterval(float seconds)
    {
        GameManager.gameManagerInstance.SetEnemySpawnRate((int)seconds);
    }

    public void ResetUI()
    {
        ToggleHUD(false);
        ToggleMainMenu(true);
        ToggleMenuShell(true);
        ToggleResults(false);
        ToggleSettings(false);
    }

    public void StartMatch()
    {
        ToggleHUD(true);
        ToggleMainMenu(false);
        ToggleMenuShell(false);
        ToggleResults(false);
        ToggleSettings(false);
    }

    public void EnterSettings()
    {
        ToggleSettings(true);
        ToggleMainMenu(false);
    }

    public void ExitSettings()
    {
        ToggleSettings(false);
        ToggleMainMenu(true);
    }

    public void ExitResults()
    {
        ToggleResults(false);
        ToggleMainMenu(true);
    }

    public void ToggleMenuShell(bool state)
    {
        _menus.SetActive(state);
    }

    public void ToggleMainMenu(bool state)
    {
        _mainMenu.SetActive(state);
    }

    public void ToggleSettings(bool state)
    {
        _settingsScreen.SetActive(state);
    }

    public void ToggleResults(bool state)
    {
        _resultsScreen.SetActive(state);
    }

    public void UpdatePlayerKillsDisplay(int amount)
    {
        _playerKillsDisplay.text = amount.ToString();
    }

    public void UpdateMatchTimer(float time)
    {
        _matchTimer.text = time.ToString("F0");
    }

    public void ToggleHUD(bool state)
    {
        _hud.SetActive(state);
    }
}
