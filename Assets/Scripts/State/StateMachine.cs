using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public enum StateName
{
    Ready, // 속력 결정 + 일정 시속으로 보딩 위 이동 -> 일정 구간이 되면 Launch로 연결
    Gliding, // 활공 -> floor에 충돌하면 Landing으로 연결
    Landing // 착지
}

public class StateMachine
{
    public BaseState CurrentState { get; private set; }
    public string currentSatateName;
    private Dictionary<StateName, BaseState> states = new Dictionary<StateName, BaseState>();

    public StateMachine(StateName stateName, BaseState state)
    {
        AddState(stateName, state);
        CurrentState = GetState(stateName);
        currentSatateName = stateName.ToString();
    }

    public void AddState(StateName stateName, BaseState state)
    {
        if(!states.ContainsKey(stateName))
        {
            states.Add(stateName, state);
        }
    }

    public BaseState GetState(StateName stateName)
    {
        if (states.TryGetValue(stateName, out BaseState state))
        {
            return state;
        }
        return null;
    }

    public string GetCurrentState()
    {
        return currentSatateName;
    }

    public void DeleteState(StateName stateName)
    {
        if(states.ContainsKey(stateName))
        {
            states.Remove(stateName);
        }
    }

    public void ChangeState(StateName stateName)
    {
        CurrentState?.OnExitState(); // 현재 상태 종료
        CurrentState = GetState(stateName);
        CurrentState?.OnEnterState(); // 다음 상태 진입
        currentSatateName = stateName.ToString();
    }

    public void UpdateState()
    {
        CurrentState?.OnUpdateState();
    }

    public void FixedUpdateState()
    {
        CurrentState?.OnFixedUpdateState();
    }
}
