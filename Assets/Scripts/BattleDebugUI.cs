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
    
    private float _searchTimer = 0f;
    private float _searchInterval = 0.5f;

    void Start()
    {
        FindAndSubscribeLeaders();
    }

    void Update()
    {
        _searchTimer += Time.deltaTime;
        if (_searchTimer >= _searchInterval)
        {
            _searchTimer = 0f;
            CheckLeadersStatus();
        }
    }

    void CheckLeadersStatus()
    {
        if (_redLeader == null)
        {
            FindLeaderForTeam(BattleTeams.Red);
        }
        
        if (_blueLeader == null)
        {
            FindLeaderForTeam(BattleTeams.Blue);
        }
    }

    void FindAndSubscribeLeaders()
    {
        FindLeaderForTeam(BattleTeams.Red);
        FindLeaderForTeam(BattleTeams.Blue);
    }

    void FindLeaderForTeam(BattleTeams team)
    {
        var allUnits = FindObjectsByType<UnitBrain>(FindObjectsSortMode.None);
        
        foreach (var unit in allUnits)
        {
            if (unit.teamID == team && unit.isLeader && unit.currentHealth > 0)
            {
                if (team == BattleTeams.Red)
                {
                    _redLeader = unit;
                    _redLeader.OnStateChanged += UpdateRedText;
                    UpdateRedText(_redLeader.CurrentStateName); 
                }
                else if (team == BattleTeams.Blue)
                {
                    _blueLeader = unit;
                    _blueLeader.OnStateChanged += UpdateBlueText;
                    UpdateBlueText(_blueLeader.CurrentStateName); 
                }
                return; 
            }
        }
        
        if (team == BattleTeams.Red) redLeaderText.text = "RED LEADER:\nSearching...";
        if (team == BattleTeams.Blue) blueLeaderText.text = "BLUE LEADER:\nSearching...";
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