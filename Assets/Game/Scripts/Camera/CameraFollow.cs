using UnityEngine;

namespace MonsterSurvivor.CameraSystem
{
    [DisallowMultipleComponent]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset;
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;

        private Vector3 velocity;

        private void Start()
        {
            if (target != null)
                transform.position = GetTargetPosition();
        }

        private void LateUpdate()
        {
            if (target == null || Time.deltaTime <= 0f)
                return;

            transform.position = smoothTime <= 0f
                ? GetTargetPosition()
                : Vector3.SmoothDamp(transform.position, GetTargetPosition(), ref velocity,
                    smoothTime, Mathf.Infinity, Time.deltaTime);
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 position = target.position;
            return new Vector3(position.x + offset.x, position.y + offset.y, transform.position.z);
        }

        private void OnDisable() => velocity = Vector3.zero;
    }
}
