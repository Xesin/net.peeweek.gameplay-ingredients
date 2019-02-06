using NaughtyAttributes;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayIngredients.StateMachines
{
    public class StateMachine : MonoBehaviour
    {
        public State DefaultState;

        [ReorderableList]
        public State[] States;

        State m_CurrentState;

        void Start()
        {
            foreach (var state in States)
                state.gameObject.SetActive(false);

            SetState(DefaultState.StateName);
        }

        public void SetState(string stateName)
        {
            State newState = States.First(o => o.StateName == stateName);

            if(newState != null)
            {
                if (m_CurrentState != null)
                {
                    // Call Exit Actions
                    Callable.Call(m_CurrentState.OnStateExit, gameObject);
                    // Then finally disable old state
                    m_CurrentState.gameObject.SetActive(false);
                }

                // Switch Active new state
                newState.gameObject.SetActive(true);

                // Then Set new current state
                m_CurrentState = newState;

                // Finally, call State enter
                Callable.Call(m_CurrentState.OnStateEnter, gameObject);
            }
        }

        public void Update()
        {
            if (m_CurrentState != null)
                Callable.Call(m_CurrentState.OnStateUpdate, this.gameObject);
        }

    }
}
