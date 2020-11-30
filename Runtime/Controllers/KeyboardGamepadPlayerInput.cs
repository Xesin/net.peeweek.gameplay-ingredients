using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GameplayIngredients.Controllers
{
#if USE_INPUT_SYSTEM
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
#endif
    public class KeyboardGamepadPlayerInput : PlayerInput
    {
        public bool useKeyboardAndMouse { get; set; } = true;
        public bool useGamepad { get; set; } = true;

        [Header("Behaviour")]
        public float LookExponent = 2.0f;
        [Range(0.0f, 0.7f)]
        public float MovementDeadZone = 0.15f;
        [Range(0.0f, 0.7f)]
        public float LookDeadZone = 0.15f;

#if USE_INPUT_SYSTEM
        [Header("Input")]
        [SerializeField]
        private UnityEngine.InputSystem.PlayerInput playerInput;
        [SerializeField]
        private string moveAction = "Move";
        [SerializeField]
        private string lookAction = "Look";
        [SerializeField]
        private string jumpAction = "Jump";
#else
        [Header("Gamepad Axes")]
        public string MovementHorizontalAxis = "Horizontal";
        public string MovementVerticalAxis = "Vertical";
        public string LookHorizontalAxis = "Look X";
        public string LookVerticalAxis = "Look Y";

        [Header("Mouse Axes")]
        public string MouseHorizontalAxis = "Mouse X";
        public string MouseVerticalAxis = "Mouse Y";

        [Header("Buttons")]
        public string JumpButton = "Jump";

        public override ButtonState Jump => m_Jump;
#endif

        public override Vector2 Look => m_Look;
        public override Vector2 Movement => m_Movement;
        public override ButtonState Jump => m_Jump;

        Vector2 m_Movement;
        Vector2 m_Look;
        ButtonState m_Jump;

        private void Start()
        {
            foreach (var item in playerInput.actions.actionMaps)
            {
                InputAction moveInputAction = item.FindAction(moveAction, false);
                if(moveInputAction != null)
                {
                    moveInputAction.performed += OnMove;
                    moveInputAction.canceled += OnMove;
                }

                InputAction lookInputAction = item.FindAction(lookAction, false);
                if (lookInputAction != null)
                {
                    lookInputAction.started += OnLook;
                    lookInputAction.performed += OnLook;
                    lookInputAction.canceled += OnLook;
                }

                InputAction jumpInputAction = item.FindAction(jumpAction, false);
                if (jumpInputAction != null)
                {
                    jumpInputAction.started += OnJump;
                    jumpInputAction.performed += OnJump;
                    jumpInputAction.canceled += OnJump;
                }

                item.Disable();
            }

            playerInput.currentActionMap.Enable();
        }

        public override void UpdateInput()
        {
#if !USE_INPUT_SYSTEM
            if(useGamepad || useKeyboardAndMouse)
            {
                m_Movement = new Vector2(Input.GetAxisRaw(MovementHorizontalAxis), Input.GetAxisRaw(MovementVerticalAxis));
                if (m_Movement.magnitude < MovementDeadZone)
                    m_Movement = Vector2.zero;
            }

            m_Look = Vector2.zero;
            if(useGamepad)
            {
                Vector2 l = new Vector2(Input.GetAxisRaw(LookHorizontalAxis), Input.GetAxisRaw(LookVerticalAxis));
                Vector2 ln = l.normalized;
                float lm = Mathf.Clamp01(l.magnitude);
                m_Look += ln * Mathf.Pow(Mathf.Clamp01(lm - LookDeadZone) / (1.0f - LookDeadZone), LookExponent);
            }

            if(useKeyboardAndMouse)
                m_Look += new Vector2(Input.GetAxisRaw(MouseHorizontalAxis), Input.GetAxisRaw(MouseVerticalAxis));

            m_Jump = GetButtonState(JumpButton);
#endif
        }

#if USE_INPUT_SYSTEM
        private void OnMove(InputAction.CallbackContext context)
        {
            m_Movement = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            m_Look = context.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            bool pressed = context.ReadValueAsButton();
            if(pressed)
            {
                if(context.phase == InputActionPhase.Started)
                {
                    m_Jump = ButtonState.JustPressed;
                }
                else
                {
                    m_Jump = ButtonState.Pressed;
                }
            }
            else
            {
                m_Jump = ButtonState.Released;
            }
            
        }
#endif
    }
}
