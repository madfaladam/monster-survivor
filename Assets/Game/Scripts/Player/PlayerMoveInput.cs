using UnityEngine;
using UnityEngine.InputSystem;

namespace MonsterSurvivor.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerMoveInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;

        public Vector2 Direction => isActiveAndEnabled && moveAction != null && moveAction.enabled
            ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
            : Vector2.zero;

        private void OnEnable()
        {
            if (inputActions == null)
            {
                Debug.LogError("Assign the input actions asset to PlayerMoveInput.", this);
                enabled = false;
                return;
            }

            // Recreate the private action on every enable. This avoids retaining a
            // disabled/disposed action when the player or scene is reloaded.
            DisposeMoveAction();

            InputAction sourceAction = inputActions.FindAction("Player/Move", true);
            moveAction = sourceAction.Clone();
            moveAction.Enable();
        }

        private void OnDisable()
        {
            DisposeMoveAction();
        }

        private void OnDestroy()
        {
            DisposeMoveAction();
        }

        private void DisposeMoveAction()
        {
            if (moveAction == null)
                return;

            moveAction.Disable();
            moveAction.Dispose();
            moveAction = null;
        }
    }
}