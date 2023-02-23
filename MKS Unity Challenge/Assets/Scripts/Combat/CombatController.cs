using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    HealthManager _healthManager;

    [SerializeField] CannonController _frontCannon;
    [SerializeField] CannonController[] _sideCannons;

    [SerializeField] float _forwardFireCooldown;
    [SerializeField] float _lateralFireCooldown;

    [SerializeField] GameObject _shipCorpse;

    public  bool _forwardFireOnCooldown { get; private set; }
    public  bool _lateralFireOnCooldown { get; private set; }

    private void OnEnable()
    {
        _healthManager = GetComponent<HealthManager>();
        _healthManager.OnDeathData += Death;        
    }

    void Death(HealthManager killer, HealthManager victim)
    {
        GameManager.gameManagerInstance.ShipDestroyed(killer);

        GameManager.gameManagerInstance.StoreShipCorpse(Instantiate(_shipCorpse, transform.position, transform.rotation));        
        gameObject.SetActive(false);
    }

    public void ForwardFire()
    {
        if (_forwardFireOnCooldown || _frontCannon == null)
            return;

        _frontCannon.Shoot();

        _forwardFireOnCooldown = true;
        Run.After(_forwardFireCooldown, () => _forwardFireOnCooldown = false);
    }

    public void LateralFire()
    {
        if (_lateralFireOnCooldown || _sideCannons.Length == 0)
            return;

        foreach (var cannon in _sideCannons)
            cannon.Shoot();

        _lateralFireOnCooldown = true;
        Run.After(_lateralFireCooldown, () => _lateralFireOnCooldown = false);
    }
}
