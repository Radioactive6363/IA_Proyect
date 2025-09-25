using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform Player;
    [SerializeField] private float MouseSensiblity = 100f;
    private PlayerInputSystemActions inputActions;
    private Vector2 lookAxis;
    private float xrotation = 0f;

    public void AssignInputActions(PlayerInputSystemActions inputs)
    {
        inputActions = inputs;
        inputActions.Player.Look.performed += ctx => lookAxis = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => lookAxis = Vector2.zero;
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void CalculateDirection()
    {
        float mouseX = lookAxis.x * MouseSensiblity * Time.deltaTime;
        float mouseY = lookAxis.y * MouseSensiblity * Time.deltaTime;
        
        xrotation -= mouseY;
        xrotation = Mathf.Clamp(xrotation, -90f, 90f);
        _camera.transform.localRotation = Quaternion.Euler(xrotation, 0f, 0f);
        
        Player.Rotate(Vector3.up * mouseX);
    }

    private void Update()
    {
        CalculateDirection();
    }
}
