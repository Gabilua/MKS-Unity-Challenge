using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManagerInstance;

    HealthManager _playerHealthManager;
    EnemySpawner _enemySpawner;

    List<GameObject> _shipCorpses = new List<GameObject>();

    float _matchDuration = 60;
    float _enemySpawnRate = 5;

    bool _matchOn;
    float _matchTimer;

    int _playerKills;

    private void Awake()
    {
        if (gameManagerInstance == null)
            gameManagerInstance = this;
    }

    private void Start()
    {
        _enemySpawner = GetComponentInChildren<EnemySpawner>();

        _playerHealthManager = GetComponentInChildren<HealthManager>();

        _playerHealthManager.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_matchOn)
            return;

        UIManager.uiManagerInstance.UpdateMatchTimer(_matchTimer);

        if (_matchTimer > 0)
            _matchTimer -= Time.deltaTime /2;
        else
            EndMatch();
    }

    void SetupPlayer()
    {
        _playerHealthManager.transform.position = Vector3.zero;
        _playerHealthManager.transform.rotation = Quaternion.identity;
        _playerHealthManager.gameObject.SetActive(true);

        _playerHealthManager.OnJustDied += EndMatch;

        UIManager.uiManagerInstance.NewHealthBar(_playerHealthManager);

        _playerHealthManager.Respawn();
    }

    public void StartMatch()
    {
        _playerKills = 0;

        UIManager.uiManagerInstance.StartMatch();

        _matchTimer = _matchDuration;

        _matchOn = true;

        SetupPlayer();

        _enemySpawner.SetEnemySpawnRate(_enemySpawnRate);
        _enemySpawner.ToggleEnemySpawn(true);
    }

    void EndMatch()
    {
        UIManager.uiManagerInstance.UpdatePlayerKillsDisplay(_playerKills);
        UIManager.uiManagerInstance.ToggleMenuShell(true);
        UIManager.uiManagerInstance.ToggleResults(true);

        _playerHealthManager.OnJustDied -= EndMatch;
        _playerHealthManager.gameObject.SetActive(false);

        _matchOn = false;

        _enemySpawner.ToggleEnemySpawn(false);
        _enemySpawner.ResetEnemies();

        foreach (var shipCorpse in _shipCorpses)
            Destroy(shipCorpse);
    }

    public void SetMatchDuration(int seconds)
    {
        if (_matchOn)
            return;

        _matchDuration = seconds;
    }

    public void SetEnemySpawnRate(float seconds)
    {
        if (_matchOn)
            return;

        _enemySpawnRate = seconds;
    }

    public void ShipDestroyed(HealthManager killer)
    {
        if (killer == _playerHealthManager)
            _playerKills++;
    }

    public void StoreShipCorpse(GameObject shipCorpse)
    {
        _shipCorpses.Add(shipCorpse);
    }

    void ClearShipCorpses()
    {
        foreach (var shipCorpse in _shipCorpses)
            Destroy(shipCorpse);
    }

    private void OnApplicationQuit()
    {
        ClearShipCorpses();
    }
}
