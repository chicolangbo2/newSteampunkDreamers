using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    protected PlayerController controller { get; private set; }
    public BaseState(PlayerController controller)
    {
        this.controller = controller;
        OnEnterState();
    }

    // 
    public abstract void OnEnterState();
    public abstract void OnUpdateState();
    public abstract void OnFixedUpdateState();
    public abstract void OnExitState();
}
