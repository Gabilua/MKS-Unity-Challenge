using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] float _followSpeed;

    void Update()
    {
        if (_target)
            transform.position = Vector3.Lerp(transform.position, _target.position, _followSpeed * Time.deltaTime);
    }
}
