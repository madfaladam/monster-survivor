using System.Collections.Generic;
using MonsterSurvivor.Player;
using UnityEngine;

namespace MonsterSurvivor.Construction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class ConstructionZone : MonoBehaviour
    {
        [SerializeField] private ConstructionSite site;
        private readonly Dictionary<Collider2D, PlayerBuildInteractor> occupants =
            new Dictionary<Collider2D, PlayerBuildInteractor>();

        public ConstructionSite Site => site;

        private void OnTriggerEnter2D(Collider2D other) => Register(other);
        private void OnTriggerStay2D(Collider2D other) => Register(other);

        private void Register(Collider2D other)
        {
            if (!isActiveAndEnabled || site == null || !site.isActiveAndEnabled || site.IsBuilt)
                return;
            if (!other.TryGetComponent(out PlayerBuildInteractor interactor) ||
                !interactor.isActiveAndEnabled)
                return;

            occupants[other] = interactor;
            interactor.AddCandidate(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!occupants.TryGetValue(other, out PlayerBuildInteractor interactor))
                return;

            occupants.Remove(other);
            // A player with multiple colliders remains in range until the last one exits.
            foreach (PlayerBuildInteractor remaining in occupants.Values)
                if (remaining == interactor)
                    return;

            if (interactor != null)
                interactor.RemoveCandidate(this);
        }

        private void OnDisable()
        {
            foreach (PlayerBuildInteractor interactor in occupants.Values)
                if (interactor != null)
                    interactor.RemoveCandidate(this);
            occupants.Clear();
        }
    }
}
