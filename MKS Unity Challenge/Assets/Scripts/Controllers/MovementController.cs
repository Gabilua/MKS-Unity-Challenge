using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private CharacterController _controller;

    // Here we define the values for how the character moves;
    [Header("Character Attributes")]
    [SerializeField] private float _turnSpeed;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _constantForwardSpeed;
    [SerializeField] private float _acceleration;

    bool _isDead;

    private Vector3 _move;
    private Vector2 _input;

    private void OnEnable()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Movement();
        Rotation();

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    public void ReceiveInput(Vector2 input)
    {
        _input = Vector3.Lerp(_input, input, _acceleration * Time.deltaTime);
    }

    private void Rotation()
    {
        if (_input.y == 0)
            return;

        transform.Rotate((transform.up * _turnSpeed) * _input.x * Time.deltaTime, Space.World);
    }

    private void Movement()
    {
        _move = (transform.forward * _movementSpeed) * (_input.y+_constantForwardSpeed);


        _controller.Move(_move * Time.deltaTime);
    }
}
