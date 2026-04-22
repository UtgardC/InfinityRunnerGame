using UnityEngine;

namespace InfinityRunner
{
    public sealed class GameCoordinator : MonoBehaviour
    {
        public static GameCoordinator Instance { get; private set; }

        [Header("Config")]
        public RunnerConfig config;

        [Header("Scene References")]
        public PlayerRunnerController player;
        public WorldGenerator worldGenerator;
        public RunnerScore score;
        public RunnerInputReader inputReader;
        public RunnerCameraController cameraController;

        [Header("Hazards")]
        public LayerMask deathLayers;
        public string deathTag = "Death";

        private RunnerState state = RunnerState.GameOver;

        public RunnerState State
        {
            get { return state; }
        }

        public RunnerConfig Config
        {
            get { return config; }
        }

        public PlayerRunnerController Player
        {
            get { return player; }
        }

        public bool IsRunning
        {
            get { return state == RunnerState.Running; }
        }

        public float WorldSpeed
        {
            get { return worldGenerator != null ? worldGenerator.CurrentSpeed : 0f; }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ApplyReferences();
        }

        private void Start()
        {
            RestartRun();
        }

        public void RestartRun()
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            ApplyReferences();
            score.ResetScore();
            player.ResetPlayer();
            player.SetControlsLocked(false);
            worldGenerator.BeginRun();

            if (cameraController != null)
            {
                cameraController.SnapToRunner();
            }

            state = RunnerState.Running;
        }

        public void RequestRestart()
        {
            RestartRun();
        }

        public void RequestLaneChange(int direction)
        {
            if (state != RunnerState.Running || player == null)
            {
                return;
            }

            player.ChangeLane(direction);
        }

        public void RequestJump()
        {
            if (state == RunnerState.Running && player != null)
            {
                player.Jump();
            }
        }

        public void AddDistance(float meters)
        {
            if (state == RunnerState.Running && score != null)
            {
                score.AddDistance(meters);
            }
        }

        public void HandlePlayerContact(Collider other)
        {
            if (state != RunnerState.Running || other == null)
            {
                return;
            }

            RunnerInteractable interactable = RunnerCollisionUtility.FindInteractable(other);
            if (interactable != null)
            {
                HandleInteractable(interactable);
                return;
            }

            if (RunnerCollisionUtility.IsDeathObject(other, deathLayers, deathTag))
            {
                GameOver();
            }
        }

        public void GameOver()
        {
            if (state == RunnerState.GameOver)
            {
                return;
            }

            state = RunnerState.GameOver;

            if (worldGenerator != null)
            {
                worldGenerator.StopRun();
            }

            if (player != null)
            {
                player.SetControlsLocked(true);
            }
        }

        private void HandleInteractable(RunnerInteractable interactable)
        {
            if (interactable == null || interactable.IsConsumed || config == null || score == null)
            {
                return;
            }

            switch (interactable.interactionType)
            {
                case RunnerInteractableType.Person:
                    interactable.Consume();
                    score.AddBonus(interactable.ResolveScore(config.personScore));
                    interactable.gameObject.SetActive(false);
                    break;
                case RunnerInteractableType.Destructible:
                    interactable.Consume();
                    score.AddBonus(interactable.ResolveScore(config.destructibleScore));
                    BreakDestructible(interactable);
                    break;
                case RunnerInteractableType.Death:
                    GameOver();
                    break;
            }
        }

        private void BreakDestructible(RunnerInteractable interactable)
        {
            DestructibleProp prop = interactable.GetComponentInParent<DestructibleProp>();
            if (prop != null)
            {
                prop.Break();
            }
            else
            {
                interactable.gameObject.SetActive(false);
            }
        }

        private bool HasRequiredReferences()
        {
            if (config == null || player == null || worldGenerator == null || score == null || inputReader == null)
            {
                Debug.LogError("GameCoordinator is missing references. Assign config, player, worldGenerator, score and inputReader in the inspector.", this);
                return false;
            }

            return true;
        }

        private void ApplyReferences()
        {
            if (score != null)
            {
                score.config = config;
            }

            if (inputReader != null)
            {
                inputReader.config = config;
                inputReader.coordinator = this;
            }

            if (worldGenerator != null)
            {
                worldGenerator.config = config;
            }

            if (player != null)
            {
                player.config = config;
                player.SetCoordinator(this);
            }

            if (cameraController != null && player != null)
            {
                cameraController.player = player.transform;
            }
        }
    }
}
