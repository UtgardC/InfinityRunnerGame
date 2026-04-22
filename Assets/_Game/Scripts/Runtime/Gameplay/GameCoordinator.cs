using System.Collections;
using System.Collections.Generic;
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
        public MainMenuUI mainMenuUI;
        public GameUI gameUI;

        [Header("Transitions")]
        [Min(0.01f)] public float startTransitionDuration = 1.2f;
        [Min(0.01f)] public float returnToMenuFadeDuration = 0.35f;

        [Header("Power Ups")]
        public PowerUpDefinition[] powerUpDefinitions;
        [Min(1)] public int minBlocksBetweenPowerUpAttempts = 3;
        [Min(1)] public int maxBlocksBetweenPowerUpAttempts = 5;

        [Header("Hazards")]
        public LayerMask deathLayers;
        public string deathTag = "Death";

        private readonly List<PowerUpDefinition> weightedPowerUpCandidates = new List<PowerUpDefinition>();
        private Coroutine stateRoutine;
        private RunnerState state = RunnerState.Menu;
        private int blocksUntilNextPowerUpAttempt;
        private bool pendingPowerUpAttempt;
        private float invincibleRemaining;
        private float scoreMultiplierRemaining;
        private int activeScoreMultiplier = 1;

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
            get { return state == RunnerState.TransitionToRun || state == RunnerState.Running; }
        }

        public float WorldSpeed
        {
            get { return worldGenerator != null ? worldGenerator.EffectiveSpeed : 0f; }
        }

        public bool HasInvincibility
        {
            get { return invincibleRemaining > 0f; }
        }

        public int ActiveScoreMultiplier
        {
            get { return Mathf.Max(1, activeScoreMultiplier); }
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

        private void Update()
        {
            UpdateActivePowerUps();
        }

        private void Start()
        {
            EnterMenuImmediate();
        }

        public void StartGameFromMenu()
        {
            if (state != RunnerState.Menu)
            {
                return;
            }

            StartStateRoutine(StartGameRoutine());
        }

        public void RequestReturnToMenu()
        {
            if (state == RunnerState.Menu || state == RunnerState.TransitionToMenu)
            {
                return;
            }

            StartStateRoutine(ReturnToMenuRoutine());
        }

        public void RequestRestart()
        {
            if (state != RunnerState.GameOver)
            {
                return;
            }

            RestartGameplayImmediate();
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

        public void RequestFastFall()
        {
            if (state == RunnerState.Running && player != null)
            {
                player.FastFall();
            }
        }

        public void AddDistance(float meters)
        {
            if (IsRunning && score != null)
            {
                score.AddDistance(meters);
            }
        }

        public void NotifyBlockSpawned(GameObject blockRoot, bool countsAsProgress)
        {
            if (!countsAsProgress || blockRoot == null)
            {
                return;
            }

            if (!pendingPowerUpAttempt)
            {
                blocksUntilNextPowerUpAttempt--;
                if (blocksUntilNextPowerUpAttempt > 0)
                {
                    return;
                }

                pendingPowerUpAttempt = true;
            }

            TrySpawnPendingPowerUp(blockRoot);
        }

        public void HandlePlayerContact(Collider other)
        {
            if (!IsRunning || other == null)
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
                if (!HasInvincibility)
                {
                    GameOver();
                }
            }
        }

        public void GameOver()
        {
            if (state == RunnerState.GameOver)
            {
                return;
            }

            StopStateRoutine();
            state = RunnerState.GameOver;

            if (worldGenerator != null)
            {
                worldGenerator.StopRun();
            }

            if (player != null)
            {
                player.SetControlsLocked(true);
            }

            if (gameUI != null)
            {
                gameUI.ShowGameOver();
            }
        }

        private void EnterMenuImmediate()
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            StopStateRoutine();
            ApplyReferences();
            ResetPowerUps();
            score.ResetScore();
            player.ResetPlayer();
            player.SetControlsLocked(true);
            worldGenerator.PrepareMenuPreview();
            cameraController.SnapToMenu();
            state = RunnerState.Menu;

            if (mainMenuUI != null)
            {
                mainMenuUI.HideBlackScreenImmediate();
                mainMenuUI.ShowMenu();
            }

            if (gameUI != null)
            {
                gameUI.ShowMenuState();
            }
        }

        private void RestartGameplayImmediate()
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            StopStateRoutine();
            ApplyReferences();
            ResetPowerUps();
            score.ResetScore();
            player.ResetPlayer();
            player.SetControlsLocked(false);
            worldGenerator.RestartGameplay(1f);
            cameraController.SnapToRunner();
            state = RunnerState.Running;

            if (mainMenuUI != null)
            {
                mainMenuUI.HideMenu();
                mainMenuUI.HideBlackScreenImmediate();
            }

            if (gameUI != null)
            {
                gameUI.ShowGameplay();
            }
        }

        private IEnumerator StartGameRoutine()
        {
            if (!HasRequiredReferences())
            {
                yield break;
            }

            ApplyReferences();
            ResetPowerUps();
            score.ResetScore();
            player.ResetPlayer();
            player.SetControlsLocked(true);
            float transitionDuration = cameraController.ResolveTransitionDuration(startTransitionDuration);
            worldGenerator.BeginRunFromPreview(0f);
            cameraController.TransitionToRunner(transitionDuration);
            state = RunnerState.TransitionToRun;

            if (mainMenuUI != null)
            {
                mainMenuUI.HideMenu();
                mainMenuUI.HideBlackScreenImmediate();
            }

            if (gameUI != null)
            {
                gameUI.ShowGameplay();
            }

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                worldGenerator.SetSpeedScale(Mathf.Clamp01(elapsed / transitionDuration));
                yield return null;
            }

            worldGenerator.SetSpeedScale(1f);
            player.SetControlsLocked(false);
            state = RunnerState.Running;
            stateRoutine = null;
        }

        private IEnumerator ReturnToMenuRoutine()
        {
            if (!HasRequiredReferences())
            {
                yield break;
            }

            state = RunnerState.TransitionToMenu;

            if (player != null)
            {
                player.SetControlsLocked(true);
            }

            if (worldGenerator != null)
            {
                worldGenerator.StopRun();
            }

            if (mainMenuUI != null)
            {
                yield return mainMenuUI.FadeBlackScreenRoutine(1f, returnToMenuFadeDuration);
            }

            ApplyReferences();
            ResetPowerUps();
            score.ResetScore();
            player.ResetPlayer();
            player.SetControlsLocked(true);
            worldGenerator.PrepareMenuPreview();
            cameraController.SnapToMenu();
            state = RunnerState.Menu;

            if (gameUI != null)
            {
                gameUI.ShowMenuState();
            }

            if (mainMenuUI != null)
            {
                mainMenuUI.ShowMenu();
                yield return mainMenuUI.FadeBlackScreenRoutine(0f, returnToMenuFadeDuration);
            }

            stateRoutine = null;
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
                    AddInteractionScore(interactable.ResolveScore(config.personScore));
                    interactable.gameObject.SetActive(false);
                    break;
                case RunnerInteractableType.Destructible:
                    interactable.Consume();
                    AddInteractionScore(interactable.ResolveScore(config.destructibleScore));
                    BreakDestructible(interactable);
                    break;
                case RunnerInteractableType.Death:
                    if (!HasInvincibility)
                    {
                        GameOver();
                    }
                    break;
                case RunnerInteractableType.PowerUp:
                    CollectPowerUp(interactable);
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

        private void CollectPowerUp(RunnerInteractable interactable)
        {
            PowerUpPickup pickup = interactable.GetComponentInParent<PowerUpPickup>();
            if (pickup == null || pickup.Definition == null)
            {
                interactable.gameObject.SetActive(false);
                return;
            }

            interactable.Consume();
            ApplyPowerUp(pickup.Definition);
            pickup.Consume();
        }

        private void ApplyPowerUp(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            switch (definition.type)
            {
                case PowerUpType.InvincibleRock:
                    invincibleRemaining = Mathf.Max(invincibleRemaining, definition.durationSeconds);
                    break;
                case PowerUpType.ScoreMultiplier:
                    scoreMultiplierRemaining = Mathf.Max(scoreMultiplierRemaining, definition.durationSeconds);
                    activeScoreMultiplier = Mathf.Max(activeScoreMultiplier, Mathf.Max(2, definition.scoreMultiplierValue));
                    break;
            }
        }

        private void UpdateActivePowerUps()
        {
            if (!IsRunning)
            {
                return;
            }

            if (invincibleRemaining > 0f)
            {
                invincibleRemaining = Mathf.Max(0f, invincibleRemaining - Time.deltaTime);
            }

            if (scoreMultiplierRemaining > 0f)
            {
                scoreMultiplierRemaining = Mathf.Max(0f, scoreMultiplierRemaining - Time.deltaTime);
                if (scoreMultiplierRemaining <= 0f)
                {
                    activeScoreMultiplier = 1;
                }
            }
        }

        private void ResetPowerUps()
        {
            pendingPowerUpAttempt = false;
            invincibleRemaining = 0f;
            scoreMultiplierRemaining = 0f;
            activeScoreMultiplier = 1;
            ScheduleNextPowerUpAttempt();
        }

        private void ScheduleNextPowerUpAttempt()
        {
            int minValue = Mathf.Max(1, minBlocksBetweenPowerUpAttempts);
            int maxValue = Mathf.Max(minValue, maxBlocksBetweenPowerUpAttempts);
            blocksUntilNextPowerUpAttempt = Random.Range(minValue, maxValue + 1);
        }

        private void TrySpawnPendingPowerUp(GameObject blockRoot)
        {
            if (!pendingPowerUpAttempt || blockRoot == null)
            {
                return;
            }

            PowerUpSpawnPoint[] spawnPoints = blockRoot.GetComponentsInChildren<PowerUpSpawnPoint>(true);
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return;
            }

            PowerUpDefinition definition = SelectPowerUpDefinition(spawnPoints);
            if (definition == null)
            {
                return;
            }

            PowerUpSpawnPoint spawnPoint = SelectSpawnPoint(spawnPoints, definition);
            if (spawnPoint == null)
            {
                return;
            }

            spawnPoint.Spawn(definition);
            pendingPowerUpAttempt = false;
            ScheduleNextPowerUpAttempt();
        }

        private PowerUpDefinition SelectPowerUpDefinition(PowerUpSpawnPoint[] spawnPoints)
        {
            weightedPowerUpCandidates.Clear();

            if (powerUpDefinitions == null)
            {
                return null;
            }

            for (int i = 0; i < powerUpDefinitions.Length; i++)
            {
                PowerUpDefinition definition = powerUpDefinitions[i];
                if (definition == null || definition.pickupPrefab == null || definition.spawnWeight <= 0)
                {
                    continue;
                }

                if (!HasCompatibleSpawnPoint(spawnPoints, definition))
                {
                    continue;
                }

                for (int weightIndex = 0; weightIndex < definition.spawnWeight; weightIndex++)
                {
                    weightedPowerUpCandidates.Add(definition);
                }
            }

            if (weightedPowerUpCandidates.Count == 0)
            {
                return null;
            }

            return weightedPowerUpCandidates[Random.Range(0, weightedPowerUpCandidates.Count)];
        }

        private PowerUpSpawnPoint SelectSpawnPoint(PowerUpSpawnPoint[] spawnPoints, PowerUpDefinition definition)
        {
            int compatibleCount = 0;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null && spawnPoints[i].CanSpawn(definition))
                {
                    compatibleCount++;
                }
            }

            if (compatibleCount == 0)
            {
                return null;
            }

            int selectedIndex = Random.Range(0, compatibleCount);
            int currentIndex = 0;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                PowerUpSpawnPoint spawnPoint = spawnPoints[i];
                if (spawnPoint == null || !spawnPoint.CanSpawn(definition))
                {
                    continue;
                }

                if (currentIndex == selectedIndex)
                {
                    return spawnPoint;
                }

                currentIndex++;
            }

            return null;
        }

        private bool HasCompatibleSpawnPoint(PowerUpSpawnPoint[] spawnPoints, PowerUpDefinition definition)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null && spawnPoints[i].CanSpawn(definition))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddInteractionScore(int basePoints)
        {
            if (score == null || basePoints <= 0)
            {
                return;
            }

            score.AddBonus(basePoints * ActiveScoreMultiplier);
        }

        private void StartStateRoutine(IEnumerator routine)
        {
            StopStateRoutine();
            stateRoutine = StartCoroutine(routine);
        }

        private void StopStateRoutine()
        {
            if (stateRoutine != null)
            {
                StopCoroutine(stateRoutine);
                stateRoutine = null;
            }
        }

        private bool HasRequiredReferences()
        {
            if (config == null
                || player == null
                || worldGenerator == null
                || score == null
                || inputReader == null
                || cameraController == null
                || mainMenuUI == null
                || gameUI == null)
            {
                Debug.LogError("GameCoordinator is missing references. Assign config, player, worldGenerator, score, inputReader, cameraController, mainMenuUI and gameUI in the inspector.", this);
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

            if (mainMenuUI != null)
            {
                mainMenuUI.coordinator = this;
            }

            if (gameUI != null)
            {
                gameUI.coordinator = this;
                gameUI.score = score;
            }
        }
    }
}
