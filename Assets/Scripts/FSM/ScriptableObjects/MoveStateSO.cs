using UnityEngine;

[CreateAssetMenu(menuName = "FSM/States/Move")]
public class MoveStateSO : StateSO<PlayerController>
{
    public override void Enter(PlayerController player) { }

    public override void Execute(PlayerController player)
    {
        var axis = player.InputAxis;
        if (axis != Vector2.zero)
        {
            Vector3 dir = new Vector3(axis.x, 0, axis.y);
            player.Move(dir.normalized);
            player.Look(dir);
        }
    }

    public override void Exit(PlayerController player) { }
}
