using UnityEngine;

namespace MonsterSurvivor.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(EnemyController))]
    public sealed class EnemyAnimation : MonoBehaviour
    {
        private static readonly int DirectionId = Animator.StringToHash("Direction");
        private static readonly int IsMovingId = Animator.StringToHash("IsMoving");
        private Animator animator;
        private EnemyController enemy;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            enemy = GetComponent<EnemyController>();
        }

        private void Update()
        {
            Vector2 direction = enemy.MoveDirection;
            bool moving = direction.sqrMagnitude > 0.001f;
            animator.SetBool(IsMovingId, moving);
            if (moving)
                animator.SetInteger(DirectionId, Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                    ? (direction.x > 0f ? 2 : 3) : (direction.y > 0f ? 1 : 0));
        }

        private void OnDisable()
        {
            if (animator != null)
                animator.SetBool(IsMovingId, false);
        }
    }
}
