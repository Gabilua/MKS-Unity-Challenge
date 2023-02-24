using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public delegate void DeathData(HealthManager killer = null, HealthManager victim = null);
    public event DeathData OnDeathData;

    public delegate void JustDied();
    public event JustDied OnJustDied;

    public delegate void Respawned(HealthManager victim);
    public event Respawned OnRespawned;

    public delegate void ReceivedDamage(HealthManager aggressor);
    public event ReceivedDamage OnReceivedDamage;

    public float _currentHealth { get; private set; }
    public float _maxHealth;
    float _regenerationRate;
    float _regenerationCooldown;

    public bool IsDead { get; private set; }

    HealthManager _lastAggressor;

    float _regenerationTimer;

    #region Unity

    private void OnEnable()
    {
        _currentHealth = _maxHealth;
    }

    private void Update()
    {     
        HealthRegeneration();
    }

    private void OnDisable()
    {
        Respawn();
    }

    #endregion

    #region Health Management

    // handles the timer for last time avatar was attacked and health regeneration
    void HealthRegeneration()
    {
        if (_regenerationRate == 0 || _currentHealth == _maxHealth)
            return;

        if (_regenerationTimer > 0)
            _regenerationTimer -= Time.deltaTime;
        else if (_regenerationTimer < 0)
            _regenerationTimer = 0;
        else
        {
            if (_currentHealth < _maxHealth)
                _currentHealth += _regenerationRate * Time.deltaTime;
            else if (_currentHealth > _maxHealth)
                _currentHealth = _maxHealth;
        }
    }

    // main method for changing health value
    public void ChangeHealthPoints(float _changeAmount)
    {
        _currentHealth += _changeAmount;
        _currentHealth = Mathf.CeilToInt(_currentHealth);

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Death();
        }
        else if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    // method for depleting health
    public void Damage(float _amount, HealthManager aggressor = null)
    {
        if (_amount == 0)
            return;

        _lastAggressor = aggressor;
        OnReceivedDamage?.Invoke(aggressor);

        _regenerationTimer = _regenerationCooldown;

        ChangeHealthPoints(-Mathf.CeilToInt(Mathf.Abs(_amount)));
    }

    // method for replenishing health
    public void Heal(float _amount)
    {
        ChangeHealthPoints(_amount);
    }

    // on health zeroed, updates visuals and disables collider to avoid being detected by enemies while dead
    void Death()
    {
        if (!IsDead)
        {
            IsDead = true;

            GetComponent<Collider>().enabled = false;

            OnDeathData?.Invoke(_lastAggressor, this);
            OnJustDied?.Invoke();
        }
    }

    // resets status, visuals and takes player back to a set point
    public void Respawn()
    {
       transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
       gameObject.SetActive(true);

        IsDead = false;

        GetComponent<Collider>().enabled = true;

        Heal(_maxHealth);
    }  
    
    #endregion
}



