using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MonsterSurvivor.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatEncounter))]
    public sealed class CombatInput : MonoBehaviour
    {
        private CombatEncounter encounter;
        private InputAction begin;
        private InputAction restart;

        private void Awake()
        {
            encounter = GetComponent<CombatEncounter>();
            begin = new InputAction("BeginCombat", InputActionType.Button, "<Keyboard>/space");
            restart = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");
        }

        private void OnEnable()
        {
            begin.Enable();
            restart.Enable();
        }

        private void Update()
        {
            if (restart.WasPressedThisFrame())
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            if (begin.WasPressedThisFrame())
                encounter.Begin();
        }

        private void OnDisable()
        {
            begin.Disable();
            restart.Disable();
        }

        private void OnDestroy()
        {
            begin.Dispose();
            restart.Dispose();
        }
    }
}
