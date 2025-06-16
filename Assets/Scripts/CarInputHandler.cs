using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputHandler : MonoBehaviour
{
    private TopDownCarController carController;
    private PlayerInput playerInput;

    void Awake()
    {
        carController = GetComponent<TopDownCarController>();
        playerInput = GetComponent<PlayerInput>();

        // Configuración programática para evitar errores de eventos
        ConfigureInputActions();
    }

    private void ConfigureInputActions()
    {
        // Busca la acción "Move" y asigna el manejador
        InputAction moveAction = playerInput.actions.FindAction("Move");
        if (moveAction != null)
        {
            moveAction.performed += HandleMoveInput;
            moveAction.canceled += HandleMoveInput;
        }
        
    }

    private void OnEnable()
    {
        // Habilita las acciones al activar el componente
        if (playerInput.actions != null)
        {
            playerInput.actions.Enable();
        }
    }

    private void OnDisable()
    {
        // Deshabilita las acciones al desactivar el componente
        if (playerInput.actions != null)
        {
            playerInput.actions.Disable();
        }
    }

    private void HandleMoveInput(InputAction.CallbackContext context)
    {
        if (carController != null)
        {
            Vector2 input = context.ReadValue<Vector2>();
            carController.SetInputVector(input);
        }
    }
}