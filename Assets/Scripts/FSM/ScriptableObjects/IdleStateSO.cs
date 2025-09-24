using UnityEngine;

[CreateAssetMenu(menuName = "FSM/States/Idle")]
public class IdleStateSO : StateSO<PlayerController>
{
    public override void Enter(PlayerController player)
    {
        player.Move(Vector3.zero);
    }

    public override void Execute(PlayerController player) { }

    public override void Exit(PlayerController player) { }
}
