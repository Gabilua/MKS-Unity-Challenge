using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    MovementController _playerMovementController;
    CombatController _combatController;
    Vector2 _movementInput;

    private void Start()
    {
        _playerMovementController = GetComponent<MovementController>();
        _combatController = GetComponent<CombatController>();
    }

    void InputManagement()
    {
        _movementInput.x = Input.GetAxis("Horizontal");
        _movementInput.y = Input.GetAxis("Vertical");
        _movementInput.y = Mathf.Clamp(_movementInput.y, 0, 1);

        _playerMovementController.ReceiveInput(_movementInput);

        if (Input.GetButtonDown("Fire1"))
            _combatController.ForwardFire();

        if (Input.GetButtonDown("Fire2"))
            _combatController.LateralFire();
    }

    void Update()
    {
        InputManagement();
    }
}
