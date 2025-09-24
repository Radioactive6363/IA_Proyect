using UnityEngine;

public class MovePlayerState : State<PlayerStates>
{
    private IMove _move;
    private IControllerInput _controllerInput;

    public MovePlayerState(FSM<PlayerStates> fsm, IMove move, IControllerInput controllerInput)
    {
        _fsm = fsm;
        _move = move;
        _controllerInput = controllerInput;
    }

    public override void Execute()
    {
        base.Execute();
        
        if (_controllerInput.MoveAxis != Vector2.zero)
        {
            Vector3 dir = new Vector3(_controllerInput.MoveAxis.x, 0, _controllerInput.MoveAxis.y);
            _move.Move(dir.normalized);
            //_move.Look(dir);
        }
        else
            _fsm.SetState(PlayerStates.Idle);

        if (Input.GetKeyDown(KeyCode.Space))
            _fsm.SetState(PlayerStates.Spining);
    }
}
