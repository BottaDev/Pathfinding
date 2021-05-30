using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private readonly Dictionary<string, State> _stateDictionary = new Dictionary<string, State>();

    private State _currentState = new EmptyState();

    public void OnUpdate()
    {
        _currentState.OnUpdate();
    }
    
    public void ChangeState(string id)
    {
        _currentState = _stateDictionary[id];
    }

    public void AddState(string id, State state)
    {
        _stateDictionary.Add(id, state);
    }
}

public class EmptyState : State
{
    public void OnUpdate() { }
}
