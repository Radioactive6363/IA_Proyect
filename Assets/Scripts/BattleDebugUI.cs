using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleDebugUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI redLeaderText;
    public TextMeshProUGUI blueLeaderText;

    private UnitBrain _redLeader;
    private UnitBrain _blueLeader;

    void Start()
    {
        FindAndSubscribeLeaders();
    }

    void FindAndSubscribeLeaders()
    {
        var allUnits = FindObjectsByType<UnitBrain>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.isLeader)
            {
                if (unit.teamID == BattleTeams.Red)
                {
                    _redLeader = unit;
                    _redLeader.OnStateChanged += UpdateRedText; 
                    UpdateRedText(_redLeader.CurrentStateName);
                }
                else if (unit.teamID == BattleTeams.Blue)
                {
                    _blueLeader = unit;
                    _blueLeader.OnStateChanged += UpdateBlueText;
                    UpdateBlueText(_blueLeader.CurrentStateName);
                }
            }
        }
    }
    
    void UpdateRedText(string stateName)
    {
        if (redLeaderText != null) 
            redLeaderText.text = $"RED LEADER:\n{stateName}";
    }

    void UpdateBlueText(string stateName)
    {
        if (blueLeaderText != null) 
            blueLeaderText.text = $"BLUE LEADER:\n{stateName}";
    }
    
    void OnDestroy()
    {
        if (_redLeader != null) _redLeader.OnStateChanged -= UpdateRedText;
        if (_blueLeader != null) _blueLeader.OnStateChanged -= UpdateBlueText;
    }
}