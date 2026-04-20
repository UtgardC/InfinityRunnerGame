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
            get { return Mathf.FloorToInt(distance * config.distanceScorePerMeter) + bonusScore; }
        }

        public float Distance
        {
            get { return distance; }
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
