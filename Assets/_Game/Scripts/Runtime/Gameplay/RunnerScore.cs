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
            get
            {
                float distanceMultiplier = config != null ? config.distanceScorePerMeter : 0f;
                return Mathf.FloorToInt(distance * distanceMultiplier) + bonusScore;
            }
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

        public void AddDistance(float meters)
        {
            if (config == null || meters <= 0f)
            {
                return;
            }

            distance += meters;
        }

        public void AddBonus(int points)
        {
            bonusScore += points;
        }
    }
}
