using TMPro;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class GameUI : MonoBehaviour
    {
        private const float MetersToMiles = 0.000621371f;

        public GameCoordinator coordinator;
        public RunnerScore score;

        [Header("Roots")]
        public GameObject hudRoot;
        public GameObject gameOverRoot;

        [Header("Labels")]
        public TMP_Text scoreText;
        public TMP_Text milesText;

        private void Update()
        {
            if (score == null)
            {
                return;
            }

            if (scoreText != null)
            {
                scoreText.text = score.TotalScore.ToString();
            }

            if (milesText != null)
            {
                float miles = score.Distance * MetersToMiles;
                milesText.text = miles.ToString("0.00");
            }
        }

        public void ShowMenuState()
        {
            SetActive(hudRoot, false);
            SetActive(gameOverRoot, false);
        }

        public void ShowGameplay()
        {
            SetActive(hudRoot, true);
            SetActive(gameOverRoot, false);
        }

        public void ShowGameOver()
        {
            SetActive(hudRoot, true);
            SetActive(gameOverRoot, true);
        }

        public void ReturnToMenuButton()
        {
            if (coordinator != null)
            {
                coordinator.RequestReturnToMenu();
            }
        }

        private void SetActive(GameObject target, bool value)
        {
            if (target != null)
            {
                target.SetActive(value);
            }
        }
    }
}
