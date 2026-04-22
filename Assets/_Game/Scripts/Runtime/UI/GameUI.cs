using TMPro;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class GameUI : MonoBehaviour
    {
        public GameCoordinator coordinator;
        public RunnerScore score;

        [Header("Roots")]
        public GameObject hudRoot;
        public GameObject gameOverRoot;

        [Header("Labels")]
        public TMP_Text scoreText;
        public TMP_Text milesText;

        [Header("Power Up HUD")]
        public TMP_Text scoreMultiplierText;
        [Min(0f)] public float scoreMultiplierPulseVariance = 0.12f;
        [Min(0f)] public float scoreMultiplierPulseSpeed = 5f;

        private Vector3 multiplierBaseScale;
        private bool hasMultiplierBaseScale;

        private void Awake()
        {
            CacheMultiplierBaseScale();
        }

        private void Update()
        {
            CacheMultiplierBaseScale();

            if (score == null)
            {
                UpdateScoreMultiplierIndicator();
                return;
            }

            if (scoreText != null)
            {
                scoreText.text = score.TotalScore.ToString();
            }

            if (milesText != null)
            {
                milesText.text = Mathf.FloorToInt(score.Distance).ToString();
            }

            UpdateScoreMultiplierIndicator();
        }

        public void ShowMenuState()
        {
            SetActive(hudRoot, false);
            SetActive(gameOverRoot, false);
            HideScoreMultiplierIndicator();
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
            HideScoreMultiplierIndicator();
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

        private void UpdateScoreMultiplierIndicator()
        {
            if (scoreMultiplierText == null)
            {
                return;
            }

            bool isActive = coordinator != null
                && coordinator.IsRunning
                && coordinator.ActiveScoreMultiplier > 1;

            if (!isActive)
            {
                HideScoreMultiplierIndicator();
                return;
            }

            if (!scoreMultiplierText.gameObject.activeSelf)
            {
                scoreMultiplierText.gameObject.SetActive(true);
            }

            scoreMultiplierText.text = "X" + coordinator.ActiveScoreMultiplier;

            float pulse = Mathf.Sin(Time.unscaledTime * scoreMultiplierPulseSpeed) * scoreMultiplierPulseVariance;
            float scaleMultiplier = Mathf.Max(0.01f, 1f + pulse);
            scoreMultiplierText.rectTransform.localScale = multiplierBaseScale * scaleMultiplier;
        }

        private void HideScoreMultiplierIndicator()
        {
            if (scoreMultiplierText == null)
            {
                return;
            }

            scoreMultiplierText.rectTransform.localScale = multiplierBaseScale;
            if (scoreMultiplierText.gameObject.activeSelf)
            {
                scoreMultiplierText.gameObject.SetActive(false);
            }
        }

        private void CacheMultiplierBaseScale()
        {
            if (hasMultiplierBaseScale || scoreMultiplierText == null)
            {
                return;
            }

            multiplierBaseScale = scoreMultiplierText.rectTransform.localScale;
            hasMultiplierBaseScale = true;
        }
    }
}
