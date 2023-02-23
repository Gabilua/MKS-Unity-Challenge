using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviourManager : MonoBehaviour
{
    HealthManager _healthManager;
    CombatController _combatController;
    MovementController _movementController;

    [SerializeField] LayerMask _targetLayer;
    [SerializeField] LayerMask _obstacleLayer;
    [SerializeField] float _detectionDistance;
    [SerializeField] float _pursueDistance;
    [SerializeField] float _engageDistance;
    [SerializeField] Vector2 _obstacleAvoidance;

    HealthManager _currentTarget;

    float _shortestDistance;

    bool isDead;

    #region Unity

    private void OnEnable()
    {
        _healthManager = GetComponent<HealthManager>();
        _healthManager.OnJustDied += Death;

        _combatController = GetComponent<CombatController>();
        _movementController = GetComponent<MovementController>();

        _currentTarget = null;
        isDead = false;
    }

    private void Update()
    {
        if (isDead)
        {
            _currentTarget = null;
            return;
        }

        if (_currentTarget == null)
        {
            _currentTarget = NearbyTarget();

            _movementController.ReceiveInput(Vector2.zero);
        }
        else
        {
            if (_currentTarget.IsDead
                || DistanceToTarget(_currentTarget.transform) > _detectionDistance)
            {
                _currentTarget = null;
                return;
            }

            if (DistanceToTarget(_currentTarget.transform) > _pursueDistance)
            {
                if (ObstacleInFront() ||ObstacleToTheLeft() || ObstacleToTheRight())
                    EvadeObstacle();
                else
                    PursueTarget();
            }

            if (DistanceToTarget(_currentTarget.transform) <= _engageDistance)
                EngageTarget();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _pursueDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _engageDistance);
    }

    #endregion

    #region Targeting

    HealthManager NearbyTarget()
    {
        HealthManager nearbyTarget = null;

        Collider[] targetCandidatesNearby = Physics.OverlapSphere(transform.position, _detectionDistance, _targetLayer);

        //if detected anything targetable
        if (targetCandidatesNearby.Length > 0)
        {
            List<Collider> _validCandidatesNearby = new List<Collider>();

            //validates the contents of the nearby list before checking for distance
            foreach (var targetCandidateNearby in targetCandidatesNearby)
            {
                if (ValidateTargetCandidate(targetCandidateNearby))
                    _validCandidatesNearby.Add(targetCandidateNearby);
            }

            //converting the array of collider to an array of transforms
            Transform[] candidateTranforms = new Transform[_validCandidatesNearby.Count];

            for (int i = 0; i < candidateTranforms.Length; i++)
            {
                candidateTranforms[i] = _validCandidatesNearby[i].transform;
            }

            //sorts for the nearest candidate;
            Transform nearestCandidate = CalculateNearest(candidateTranforms);

            if (nearestCandidate != null)
                nearbyTarget = nearestCandidate.GetComponent<HealthManager>();
        }

        return nearbyTarget;
    }
    // auxiliary to calculate desired object based on distance to this
    Transform CalculateNearest(Transform[] objects)
    {
        _shortestDistance = Mathf.Infinity;
        Transform currentNearestObject = null;

        foreach (Transform currentObject in objects)
        {
            if (currentObject != transform)
            {
                float distanceToObject = (transform.position - currentObject.position).magnitude;

                if (distanceToObject < _shortestDistance)
                {
                    _shortestDistance = distanceToObject;
                    currentNearestObject = currentObject.transform;
                }
            }
        }

        return currentNearestObject;
    }

    bool ValidateTargetCandidate(Collider targetCandidate)
    {
        bool result = false;

        if (targetCandidate.GetComponent<HealthManager>()
             && targetCandidate.GetComponent<HealthManager>() != _healthManager
                   && !targetCandidate.GetComponent<HealthManager>().IsDead)
            result = true;

        return result;
    }

    bool TargetInLineOfSight(Transform target)
    {
        bool result = true;

        if (Vector3.Angle(GroundLevelTargetDirection(_currentTarget.transform), transform.forward) > 45f)
            result = false;

        return result;
    }

    bool TargetToTheSides(Transform target)
    {
        bool result = false;

        if (Vector3.Angle(GroundLevelTargetDirection(_currentTarget.transform), transform.right) <= 45f || Vector3.Angle(GroundLevelTargetDirection(_currentTarget.transform), -transform.right) <= 45f)
            result = true;

        return result;
    }

    Vector3 GroundLevelTargetDirection(Transform target)
    {
        return (GroundLevelTargetPosition(target) - transform.position).normalized;
    }

    Vector3 GroundLevelTargetPosition(Transform target)
    {
        return new Vector3(target.position.x, transform.position.y, target.position.z);
    }

    float DistanceToTarget(Transform target)
    {
        return (transform.position - GroundLevelTargetPosition(target)).magnitude;
    }

    bool ObstacleInFront()
    {
        bool result = false;

        RaycastHit hit;
        if(Physics.SphereCast(transform.position, _obstacleAvoidance.x, transform.forward, out hit, _obstacleAvoidance.y, _obstacleLayer))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            if ((hit.collider.gameObject.layer == LayerMask.NameToLayer("Ship") && hit.collider.gameObject != gameObject)
                || hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                result = true;
        }

        return result;
    }

    bool ObstacleToTheRight()
    {
        bool result = false;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, _obstacleAvoidance.x, transform.right, out hit, _obstacleAvoidance.y, _obstacleLayer))
        {
            Debug.DrawLine(transform.position, hit.point, Color.green);

            if ((hit.collider.gameObject.layer == LayerMask.NameToLayer("Ship") && hit.collider.gameObject != gameObject)
                || hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                result = true;
        }

        return result;
    }

    bool ObstacleToTheLeft()
    {
        bool result = false;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, _obstacleAvoidance.x, -transform.right, out hit, _obstacleAvoidance.y, _obstacleLayer))
        {
            Debug.DrawLine(transform.position, hit.point, Color.blue);

            if ((hit.collider.gameObject.layer == LayerMask.NameToLayer("Ship") && hit.collider.gameObject != gameObject)
                || hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                result = true;
        }

        return result;
    }

    #endregion

    #region Combat

    void EvadeObstacle()
    {
        Vector2 _movementInput = Vector2.zero;

        if(ObstacleToTheLeft())
            _movementInput.x = 2;
        else
            _movementInput.x = -2;

        _movementController.ReceiveInput(_movementInput);
    }

    void PursueTarget()
    {
        Vector2 _movementInput = Vector2.zero;

        if (Vector3.Angle(GroundLevelTargetDirection(_currentTarget.transform), transform.forward) > 45)
        {
            if (Vector3.SignedAngle(transform.position, GroundLevelTargetPosition(_currentTarget.transform), transform.forward) > 0)
                _movementInput.x = 1;
            else if (Vector3.SignedAngle(transform.position, GroundLevelTargetPosition(_currentTarget.transform), transform.forward) < 0)
                _movementInput.x = -1;
        }
        else
            _movementInput.x = 0;

        _movementInput.y = 1;       

        _movementController.ReceiveInput(_movementInput);
    }

    private void Break()
    {
        Vector2 _movementInput = Vector2.zero;  

        _movementInput.y = 0;

        _movementController.ReceiveInput(_movementInput);
    }

    void EngageTarget()
    {
        if (TargetInLineOfSight(_currentTarget.transform) && !_combatController._forwardFireOnCooldown)
            _combatController.ForwardFire();
         
        if (TargetToTheSides(_currentTarget.transform) && !_combatController._lateralFireOnCooldown)
        {
            _combatController.LateralFire();
        }
    }

    void Death()
    {
        isDead = true;
    }

    #endregion
}
