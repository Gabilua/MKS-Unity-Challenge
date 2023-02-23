using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{    
    [SerializeField] List<Transform> _spawnPoints = new List<Transform>();

    [SerializeField] Transform _enemyHolder;
    [SerializeField] GameObject[] _enemyPrefabs;
    List<GameObject> _enemyPool = new List<GameObject>();

    float _enemySpawnTime;

    float _spawnTimer;

    bool isActive;

    private void Update()
    {
        if (!isActive)
            return;

        if (_spawnTimer > 0)
            _spawnTimer -= Time.deltaTime;
        else if (_spawnTimer < 0)
            _spawnTimer = 0;
        else if (_spawnTimer == 0)
        {
            SpawnEnemy();
            _spawnTimer = _enemySpawnTime;
        }
    }

    void SpawnEnemy()
    {
        Transform point = transform;

        _spawnPoints.Shuffle();

        foreach (var spawnPoint in _spawnPoints)
        {
            if (!CheckObjectVisibility(spawnPoint.gameObject))
            {
                point = spawnPoint;
                break;
            }
        }

        GameObject enemy = AvailableEnemyInPool();
        enemy.transform.position = point.position;
        enemy.transform.rotation = point.rotation;

        enemy.transform.SetParent(_enemyHolder);
        enemy.gameObject.SetActive(true);

        HealthManager enemyHealth = enemy.GetComponent<HealthManager>();

        UIManager.uiManagerInstance.NewHealthBar(enemyHealth);

        enemyHealth.OnDeathData += OnEnemyDeath;
    }

    GameObject AvailableEnemyInPool()
    {
        GameObject availableEnemy = null;

        foreach (var enemy in _enemyPool)
        {
            if (!enemy.activeInHierarchy)
            {
                availableEnemy = enemy;
                break;
            }
        }

        if (availableEnemy == null)
        {
            GameObject enemy = Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)], transform.position, transform.rotation);

            _enemyPool.Add(enemy);
            availableEnemy = enemy;
        }

        return availableEnemy;
    }

    public bool CheckObjectVisibility(GameObject target)
    {
        bool result = true;

        Vector3 screenPos = UIManager.uiManagerInstance._camera.WorldToScreenPoint(target.transform.position);

        if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height)
            result = false;

        return result;
    }

    void OnEnemyDeath(HealthManager killer, HealthManager victim)
    {
       
    }

    public void ToggleEnemySpawn(bool state)
    {
        isActive = state;
    }
    public void SetEnemySpawnRate(float rate)
    {
        _enemySpawnTime = rate;
    }

    public void ResetEnemies()
    {
        foreach (var enemy in _enemyPool)
            enemy.SetActive(false);
    }
}
