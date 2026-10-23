using UnityEngine;

namespace MonsterSurvivor.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerMoveInput))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;

        private Rigidbody2D body;
        private PlayerMoveInput moveInput;

        public Vector2 MoveDirection { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            moveInput = GetComponent<PlayerMoveInput>();
        }

        private void FixedUpdate()
        {
            MoveDirection = moveInput.Direction;
            body.linearVelocity = MoveDirection * moveSpeed;
        }

        private void OnDisable()
        {
            MoveDirection = Vector2.zero;
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }
    }
}
