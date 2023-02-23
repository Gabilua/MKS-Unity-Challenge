using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageMode { SingleTarget, AoE }

[System.Serializable]
public class DamageInfo
{
    [Header("Combat Settings")]
    public DamageMode damageMode;
    public float finalDamageValue;
    public bool selfDamagePossible;

    public HealthManager emitter;

    [Header("Visual Settings")]
    public GameObject spawnVFX;
    public GameObject damageVFX;
    public GameObject environmentHitFX;

    public void Setup(HealthManager _emitter, float value)
    {
        emitter = _emitter;
        finalDamageValue = value;       
    }
}

public class DamageSource : MonoBehaviour
{
    public delegate void DealtKillingBlow(HealthManager victim);
    public event DealtKillingBlow OnDealtKillingBlow;

    public float _lifetime;
    public DamageInfo _damageInfo;
    public bool inactive;

    Collider _collider;
    
    List<HealthManager> _alreadyHitTargets = new List<HealthManager>();

    private void OnEnable()
    {
        _collider = GetComponent<Collider>();

        if (_damageInfo.spawnVFX != null)
            ShowSpawnVFX(_damageInfo.spawnVFX, transform.position);
    }

    public void ShowDamageVFX(GameObject vfx, Vector3 where)
    {
        GameObject fx = Instantiate(vfx, where, Quaternion.LookRotation(-transform.forward));

        if (fx.GetComponent<DamageSource>())
            fx.GetComponent<DamageSource>()._damageInfo.emitter = _damageInfo.emitter;
    }

    public void ShowSpawnVFX(GameObject vfx, Vector3 where)
    {
        GameObject fx = Instantiate(vfx, where, transform.rotation);
    }

    public void ShowEnvironmentHitFX(GameObject vfx, Vector3 where)
    {
        GameObject fx = Instantiate(vfx, where, vfx.transform.rotation);
    }

    void SuccessfulDamage(Vector3 damagePoint)
    {
        if (_damageInfo.damageVFX != null)
            ShowDamageVFX(_damageInfo.damageVFX, damagePoint);

        if (_damageInfo.damageMode == DamageMode.SingleTarget)
            Disable();
        else
            Run.After(_lifetime, ()=> Disable());
    }

    void Disable()
    {
        inactive = true;

        gameObject.SetActive(false);
    }

    // applies damage value on collision
    private void OnTriggerEnter(Collider other)
    {
        if (inactive)
            return;

        if (other.GetComponent<HealthManager>())
        {
            HealthManager health = other.GetComponent<HealthManager>();

            if (health.IsDead)
                return;

            if (_damageInfo.emitter)
            {
                if (_damageInfo.emitter == health && !_damageInfo.selfDamagePossible)
                    return;
            }

            if (_damageInfo.finalDamageValue >= health._currentHealth)
               Run.After(0.1f, () => OnDealtKillingBlow?.Invoke(health));

            health.Damage(_damageInfo.finalDamageValue, _damageInfo.emitter);

            SuccessfulDamage(other.ClosestPoint(transform.position));
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Water") && _damageInfo.environmentHitFX != null)
        {
            ShowEnvironmentHitFX(_damageInfo.environmentHitFX, transform.position);
            Disable();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            ShowDamageVFX(_damageInfo.damageVFX, transform.position);
            Disable();
        }
    }

    private void OnDisable()
    {
        inactive = false;
    }
}
