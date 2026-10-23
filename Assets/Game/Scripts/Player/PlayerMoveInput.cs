using UnityEngine;
using UnityEngine.InputSystem;

namespace MonsterSurvivor.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerMoveInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;

        public Vector2 Direction => isActiveAndEnabled && moveAction != null
            ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
            : Vector2.zero;

        private void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError("Assign the input actions asset to PlayerMoveInput.", this);
                enabled = false;
                return;
            }

            // Own the action so disabling this component cannot disable other consumers.
            moveAction = inputActions.FindAction("Player/Move", true).Clone();
        }

        private void OnEnable() => moveAction?.Enable();
        private void OnDisable() => moveAction?.Disable();
        private void OnDestroy() => moveAction?.Dispose();
    }
}
