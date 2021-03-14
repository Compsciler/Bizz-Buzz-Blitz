using System;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    internal static StateManager instance;
    private IState state;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public bool StateEquals<T>()
    {
        return state is T;
    }

    public IState GetState()
    {
        return state;
    }
    public void SetState(IState newState)
    {
        state = newState;
    }
}