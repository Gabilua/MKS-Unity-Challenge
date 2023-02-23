using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum ProjectileType { StraightForward, PhysicsBased, TweenArc, Homing };

[System.Serializable]
public class ProjectileSettings
{
    [Header("General Settings")]
    public ProjectileType _projectileType;
    public ParticleSystem _activationFX;
    public GameObject _hitFX;
    public Transform _predefinedTarget;
    public bool isActive;

    [Header("Type Specific Settings")]
    public float _travelSpeed;
    public float _throwForce;
    public float _throwPitch;
    public float _arcDuration;
    public float _arcHeight;
    public float _homingPrecision;
    public float _homingScanRadius;
    public LayerMask _homingTargetLayer;

    [HideInInspector] public Transform _homingTarget;
    [HideInInspector] public Vector3 _arcEnd;
}

[RequireComponent(typeof(Rigidbody))]
public class ProjectileController : MonoBehaviour
{
    public ProjectileSettings _projectileSettings;

    Vector3 _previousPosition;
    Rigidbody _rb;

    public void Activate(Vector3 barrelVelocity, Transform _setTarget = null)
    {
        if (_setTarget != null)
            _projectileSettings._predefinedTarget = _setTarget;

        _rb = GetComponent<Rigidbody>();

        switch (_projectileSettings._projectileType)
        {
            case ProjectileType.PhysicsBased:
                Throw(barrelVelocity);
                break;
            case ProjectileType.TweenArc:
                _projectileSettings._arcEnd = _projectileSettings._predefinedTarget.position;
                Arc();
                break;
            case ProjectileType.Homing:
                _projectileSettings._homingTarget = _projectileSettings._predefinedTarget;
                break;
            case ProjectileType.StraightForward:
                {
                    if (_projectileSettings._predefinedTarget != null)
                        transform.LookAt(_projectileSettings._predefinedTarget.position);

                    Launch();
                }
                break;
        }
    }

    private void Update()
    {
        if (!_projectileSettings.isActive)
        {
            if (_projectileSettings._projectileType == ProjectileType.Homing)
                ScanForHomingTarget();

            return;
        }

        if (_projectileSettings._projectileType == ProjectileType.Homing)
            HomeIn();
        else if (_projectileSettings._projectileType == ProjectileType.PhysicsBased)
        {
            if (_rb.velocity != Vector3.zero)
                transform.forward = _rb.velocity;
        }
    }

    private void LateUpdate()
    {
        _previousPosition = transform.position;
    }

    void Throw(Vector3 barrelVelocity)
    {
        if (_projectileSettings._activationFX)
            _projectileSettings._activationFX.Play();

        transform.rotation = Quaternion.Euler(-_projectileSettings._throwPitch, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        _rb.AddForce((transform.forward * _projectileSettings._throwForce) + barrelVelocity, ForceMode.Impulse);
        _rb.useGravity = true;
    }

    void Arc()
    {
        if (_projectileSettings._activationFX)
            _projectileSettings._activationFX.Play();

        transform.DOLocalJump(_projectileSettings._arcEnd, _projectileSettings._arcHeight, 1, _projectileSettings._arcDuration).SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                Vector3 dir = (_previousPosition - transform.position).normalized;

                transform.forward = -dir;
            })
            .OnComplete(() => { SpawnHitFX(); gameObject.SetActive(false); });
    }

    void Launch()
    {
        if (_projectileSettings._activationFX)
            _projectileSettings._activationFX.Play();

        _rb.AddForce((transform.forward * _projectileSettings._travelSpeed), ForceMode.Impulse);
        _rb.useGravity = false;
    }

    void HomeIn()
    {
        transform.Translate(transform.forward * _projectileSettings._travelSpeed * Time.deltaTime, Space.World);
        //_rb.AddForce(transform.forward * _projectileSettings._travelSpeed, ForceMode.Force);

        Vector3 lookDir = (_projectileSettings._homingTarget.position - transform.position).normalized;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), _projectileSettings._homingPrecision * Time.deltaTime);
    }

    void ScanForHomingTarget()
    {
        Collider[] _nearbyTargetCandidates = Physics.OverlapSphere(transform.position, _projectileSettings._homingScanRadius, _projectileSettings._homingTargetLayer);

        if (_nearbyTargetCandidates.Length == 0)
            return;

        _projectileSettings._homingTarget = _nearbyTargetCandidates[0].transform;

        if (_projectileSettings._activationFX)
            _projectileSettings._activationFX.Play();
    }

    void SpawnHitFX()
    {
        if (_projectileSettings._hitFX)
            Instantiate(_projectileSettings._hitFX, transform.position, _projectileSettings._hitFX.transform.rotation);
    }

    private void OnDisable()
    {
        _rb.velocity = Vector3.zero;
    }
}
