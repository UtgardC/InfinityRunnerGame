using UnityEngine;

namespace InfinityRunner
{
    public sealed class RunnerScore : MonoBehaviour
    {
        public RunnerConfig config;

        private float distance;
        private int bonusScore;

        public int TotalScore
        {
            get { return bonusScore; }
        }

        public float Distance
        {
            get { return distance; }
        }

        public int BonusScore
        {
            get { return bonusScore; }
        }

        public void ResetScore()
        {
            distance = 0f;
            bonusScore = 0;
        }

        public void AddDistance(float worldDistance)
        {
            if (config == null || worldDistance <= 0f)
            {
                return;
            }

            distance += worldDistance * Mathf.Max(1, config.metersPerSpeedUnit);
        }

        public void AddBonus(int points)
        {
            bonusScore += points;
        }
    }
}
