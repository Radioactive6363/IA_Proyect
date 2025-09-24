using UnityEngine;

public class IdlePlayerState : State<PlayerStates>
{
    private IMove _move;
    private IControllerInput _controllerInput;

    public IdlePlayerState(FSM<PlayerStates> fsm, IMove move, IControllerInput controllerInput)
    {
        _fsm = fsm;
        _move = move;
        _controllerInput = controllerInput;
    }

    public override void Enter()
    {
        base.Enter();
        _move.Move(Vector3.zero);
    }

    public override void Execute()
    {
        base.Execute();
        
        if (_controllerInput.MoveAxis != Vector2.zero)
        {
            _fsm.SetState(PlayerStates.Moving);
        }
    }
}
