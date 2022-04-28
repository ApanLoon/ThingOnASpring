using System;
using System.Collections.Generic;
using UnityEngine;

public class StateChangeBehaviour : StateMachineBehaviour
{
    private Dictionary<int, HashSet<Action<Animator, AnimatorStateInfo, int>>> _stateEnterHandlers = new Dictionary<int, HashSet<Action<Animator, AnimatorStateInfo, int>>>();
    public void RegisterOnStateEnter(int stateHash, Action<Animator, AnimatorStateInfo, int> handler)
    {
        if (!_stateEnterHandlers.ContainsKey(stateHash))
        {
            _stateEnterHandlers[stateHash] = new HashSet<Action<Animator, AnimatorStateInfo, int>>();
        }

        if (_stateEnterHandlers[stateHash].Contains(handler))
        {
            return;
        }

        _stateEnterHandlers[stateHash].Add(handler);
    }
    public void UnRegisterOnStateEnter(int stateHash, Action<Animator, AnimatorStateInfo, int> handler)
    {
        if (!_stateEnterHandlers.ContainsKey(stateHash))
        {
            return;
        }

        if (!_stateEnterHandlers[stateHash].Contains(handler))
        {
            return;
        }
        _stateEnterHandlers[stateHash].Remove(handler);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateEnterHandlers.ContainsKey(stateInfo.shortNameHash))
        {
            foreach (var action in _stateEnterHandlers[stateInfo.shortNameHash])
            {
                action(animator, stateInfo, layerIndex);
            }
        }
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
}
