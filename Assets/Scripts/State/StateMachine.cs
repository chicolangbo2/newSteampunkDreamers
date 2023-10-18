using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public enum StateName
{
    Ready, // �ӷ� ���� + ���� �ü����� ���� �� �̵� -> ���� ������ �Ǹ� Launch�� ����
    Gliding, // Ȱ�� -> floor�� �浹�ϸ� Landing���� ����
    Landing // ����
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
        CurrentState?.OnExitState(); // ���� ���� ����
        CurrentState = GetState(stateName);
        CurrentState?.OnEnterState(); // ���� ���� ����
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
