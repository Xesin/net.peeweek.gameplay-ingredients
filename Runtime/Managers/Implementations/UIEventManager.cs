using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if USE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace GameplayIngredients
{
    [RequireComponent(typeof(EventSystem))]
#if USE_INPUT_SYSTEM
    [RequireComponent(typeof(InputSystemUIInputModule))]
#else
    [RequireComponent(typeof(StandaloneInputModule))]
#endif
    [ManagerDefaultPrefab("UIEventManager")]
    public class UIEventManager : Manager
    {
        public EventSystem eventSystem { get { return m_EventSystem; } }
        [SerializeField]
        private EventSystem m_EventSystem;

        private void OnEnable()
        {
            m_EventSystem = GetComponent<EventSystem>();
        }
    }
}


