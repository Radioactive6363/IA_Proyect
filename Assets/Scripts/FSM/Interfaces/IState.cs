using UnityEngine;

public interface IState
{
    FSM fsm { get; }

    void Enter();
    void Execute();
    void Exit();
}
