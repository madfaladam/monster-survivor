using System.Text;
using MonsterSurvivor.Combat;
using MonsterSurvivor.Stats;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterSurvivor.UI
{
    [DisallowMultipleComponent]
    public sealed class GameResultUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatEncounter encounter;
        [SerializeField] private GameSessionStats stats;

        [Header("Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject finishPanel;

        [Header("Labels")]
        [SerializeField] private Text gameOverTitle;
        [SerializeField] private Text finishTitle;
        [SerializeField] private Text finishResultText;

        [Header("Restart Buttons")]
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button finishRestartButton;

        private bool resultShown;

        private void Awake()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (finishPanel != null) finishPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (encounter != null)
                encounter.Changed += HandleEncounterChanged;
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(Restart);
            if (finishRestartButton != null) finishRestartButton.onClick.AddListener(Restart);
            HandleEncounterChanged();
        }

        private void OnDisable()
        {
            if (encounter != null)
                encounter.Changed -= HandleEncounterChanged;
            if (gameOverRestartButton != null)
                gameOverRestartButton.onClick.RemoveListener(Restart);
            if (finishRestartButton != null)
                finishRestartButton.onClick.RemoveListener(Restart);
        }

        private void HandleEncounterChanged()
        {
            if (resultShown || encounter == null)
                return;

            if (encounter.State == EncounterState.Defeated)
                ShowGameOver();
            else if (encounter.State == EncounterState.Finished)
                ShowFinish();
        }

        private void ShowGameOver()
        {
            resultShown = true;
            if (gameOverTitle != null) gameOverTitle.text = "GAME OVER";
            if (finishPanel != null) finishPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        private void ShowFinish()
        {
            resultShown = true;
            if (finishTitle != null) finishTitle.text = "FINISH";
            if (finishResultText != null) finishResultText.text = BuildResultText();
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (finishPanel != null) finishPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        private string BuildResultText()
        {
            if (stats == null)
                return "Result statistics unavailable. Assign GameSessionStats in the Inspector.";

            var result = new StringBuilder();
            result.AppendLine($"Turret Built: {stats.TurretsBuilt}");
            result.AppendLine($"Turret Destroyed: {stats.TurretsDestroyed}");
            result.AppendLine();
            result.AppendLine($"Barricade Built: {stats.BarricadesBuilt}");
            result.AppendLine($"Barricade Destroyed: {stats.BarricadesDestroyed}");
            result.AppendLine();
            result.AppendLine($"Enemy Killed: {stats.EnemiesKilled}");
            result.AppendLine();
            result.AppendLine($"Total Wood: {stats.TotalWood}");
            result.Append($"Total Stone: {stats.TotalStone}");
            return result.ToString();
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
