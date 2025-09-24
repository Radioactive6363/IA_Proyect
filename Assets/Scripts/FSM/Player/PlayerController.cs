using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private StateSO<PlayerController> initialState;
    private FSM<PlayerController> fsm;

    private IMove move;
    public Vector2 InputAxis { get; private set; }
    
    private PlayerInputSystemActions inputActions;

    private void Awake()
    {
        move = GetComponent<IMove>();
        inputActions = new PlayerInputSystemActions();
    }
    
    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += ctx => InputAxis = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => InputAxis = Vector2.zero;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    
    private void Start()
    {
        fsm = new FSM<PlayerController>(this, initialState);
    }

    private void Update()
    {
        fsm.Update();
    }
    
    public void Move(Vector3 dir) => move.Move(dir);
    public void Look(Vector3 dir) => move.LookAtDir(dir);
}