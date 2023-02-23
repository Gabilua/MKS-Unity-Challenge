using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    HealthManager _healthManager;

    public Transform[] barrels;

    public GameObject _projectilePrefab;
    List<GameObject> _projectilePool = new List<GameObject>();

    [SerializeField] bool _addCannonMomentumToProjectile;

    Vector3 _previousPosition;

    private void OnEnable()
    {
        _healthManager = GetComponentInParent<HealthManager>(); 
    }

    private void LateUpdate()
    {
        _previousPosition = transform.position;
    }

    public void Shoot()
    {
        foreach (var barrel in barrels)
        {
            GameObject shotProjecilte = AvailableProjectileInPool();

            shotProjecilte.transform.position = barrel.position;
            shotProjecilte.transform.rotation = barrel.rotation;

            shotProjecilte.SetActive(true);

            Vector3 extraVelocity = Vector3.zero;

            if (_addCannonMomentumToProjectile)
                extraVelocity = BarrelVelocity();

            shotProjecilte.GetComponent<ProjectileController>().Activate(extraVelocity);
            shotProjecilte.GetComponent<DamageSource>()._damageInfo.emitter = _healthManager;
        }
    }

    GameObject AvailableProjectileInPool()
    {
        GameObject availableProjectile = null;

        foreach (var projectile in _projectilePool)
        {
            if (!projectile.activeInHierarchy)
            {
                availableProjectile = projectile;
                break;
            }
        }

        if (availableProjectile == null)
        {
            GameObject projectile = Instantiate(_projectilePrefab, transform.position, transform.rotation);

            _projectilePool.Add(projectile);
            availableProjectile = projectile;
        }

        return availableProjectile;
    }

    Vector3 BarrelVelocity()
    {
        return (transform.position - _previousPosition) / Time.deltaTime;
    }
}
