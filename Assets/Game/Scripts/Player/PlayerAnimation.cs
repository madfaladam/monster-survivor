using UnityEngine;

namespace MonsterSurvivor.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(PlayerMovement))]
    public sealed class PlayerAnimation : MonoBehaviour
    {
        private static readonly int DirectionId = Animator.StringToHash("Direction");
        private static readonly int IsMovingId = Animator.StringToHash("IsMoving");

        private Animator animator;
        private PlayerMovement movement;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            movement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            Vector2 direction = movement.MoveDirection;
            bool isMoving = direction.sqrMagnitude > 0.001f;
            animator.SetBool(IsMovingId, isMoving);
            if (!isMoving)
                return;

            int facing = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? (direction.x > 0f ? 2 : 3)
                : (direction.y > 0f ? 1 : 0);
            animator.SetInteger(DirectionId, facing);
        }

        private void OnDisable()
        {
            if (animator != null)
                animator.SetBool(IsMovingId, false);
        }
    }
}
