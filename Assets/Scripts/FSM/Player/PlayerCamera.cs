using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform Player;
    [SerializeField] private float MouseSensiblity = 100f;


    private PlayerInputSystemActions inputActions;
    private Vector2 lookAxis;


    private float xrotation = 0f;


    private void Awake()
    {
        inputActions = new PlayerInputSystemActions();
    }

    private void OnDisable()
    {
        inputActions.Player.Look.performed -= ctx => lookAxis = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= _ => lookAxis = Vector2.zero;
        inputActions.Disable();
    }

    private void Update()
    {
        float mouseX = lookAxis.x * MouseSensiblity * Time.deltaTime;
        float mouseY = lookAxis.y * MouseSensiblity * Time.deltaTime;

        // Rotación vertical (camara)
        xrotation -= mouseY;
        xrotation = Mathf.Clamp(xrotation, -90f, 90f); // evita que gire completo
        transform.localRotation = Quaternion.Euler(xrotation, 0f, 0f);

        // Rotación horizontal (cuerpo del jugador)
        Player.Rotate(Vector3.up * mouseX);
    }
}
