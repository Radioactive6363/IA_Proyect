using UnityEngine;
using UnityEngine.SceneManagement; // Para reiniciar la escena

public class SpectatorController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 10f;
    public float turboMultiplier = 3f;
    public float mouseSensitivity = 2f;
    
    private float _rotationX = 0f;
    private float _rotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Vector3 rot = transform.localRotation.eulerAngles;
        _rotationX = rot.y;
        _rotationY = rot.x;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
        HandleInput();
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _rotationX += mouseX;
        _rotationY -= mouseY;
        
        _rotationY = Mathf.Clamp(_rotationY, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_rotationY, _rotationX, 0);
    }

    void HandleMovement()
    {
        float currentSpeed = moveSpeed;
        
        if (Input.GetKey(KeyCode.LeftShift))
            currentSpeed *= turboMultiplier;
        
        float x = Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;
        
        // Movimiento vertical absoluto (Q/E)
        float y = 0;
        if (Input.GetKey(KeyCode.E)) y = currentSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q)) y = -currentSpeed * Time.deltaTime;

        transform.Translate(new Vector3(x, y, z));
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}