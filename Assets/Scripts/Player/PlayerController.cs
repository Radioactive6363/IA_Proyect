using UnityEngine;

public class PlayerController : MonoBehaviour, IControllerInput
{
    [SerializeField] PlayerCamera cameraController;
    private FSM<PlayerStates> _fsm;
    private IMove _move;
    private PlayerInputSystemActions _inputActions;
    public Vector2 MoveAxis { get; set; }

    private void Awake()
    {
        _inputActions = new PlayerInputSystemActions();
        if (cameraController != null)
        {
            cameraController.AssignInputActions(_inputActions);
        }
    }
    
    private void Start()
    {
        _move = GetComponent<IMove>();
        SetFSM();
    }
    
    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.Move.performed += ctx => MoveAxis = ctx.ReadValue<Vector2>();
        _inputActions.Player.Move.canceled += _ => MoveAxis = Vector2.zero;
    }

    public void SetFSM()
    {
        _fsm = new();
        var idle = new IdlePlayerState(_fsm, _move, this);
        var move = new MovePlayerState(_fsm, _move, this);

        idle.AddTransition(PlayerStates.Moving, move);

        move.AddTransition(PlayerStates.Idle, idle);

        _fsm.SetInitialState(idle);
    }

    void Update()
    {
        _fsm.OnUpdate();
    }
    
}
