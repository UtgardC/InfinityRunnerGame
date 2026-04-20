using System.Collections;
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
        public RunnerUI ui;

        private RunnerState state = RunnerState.Menu;
        private Coroutine destroyAllRoutine;
        private ClashController activeClash;
        private float clashPower;

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

        public float CurrentWorldSpeed
        {
            get
            {
                if (state != RunnerState.Running || worldGenerator == null)
                {
                    return 0f;
                }

                return worldGenerator.CurrentSpeed;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BindReferences();
        }

        private void Start()
        {
            RestartToMenu();
        }

        private void Update()
        {
            if (state == RunnerState.Clash)
            {
                UpdateClash();
            }

            if (ui != null && score != null && worldGenerator != null && (state == RunnerState.Running || state == RunnerState.Clash))
            {
                ui.UpdateHud(score.TotalScore, score.Distance, worldGenerator.CurrentStage);
            }
        }

        public void StartGameFromMenu()
        {
            if (state != RunnerState.Menu && state != RunnerState.GameOver)
            {
                return;
            }

            StartCoroutine(StartGameRoutine());
        }

        public void RestartToMenu()
        {
            StopAllCoroutines();
            destroyAllRoutine = null;
            activeClash = null;
            clashPower = 0f;
            state = RunnerState.Menu;

            BindReferences();

            if (worldGenerator != null)
            {
                worldGenerator.ResetGenerator();
            }

            if (score != null)
            {
                score.ResetScore();
            }

            if (player != null)
            {
                player.config = config;
                player.ResetPlayer();
                player.SetControlsLocked(true);
            }

            if (cameraController != null)
            {
                cameraController.SnapToMenu();
            }

            if (ui != null)
            {
                ui.SetCoordinator(this);
                ui.ShowMenu();
            }
        }

        public void RequestRestart()
        {
            if (state == RunnerState.GameOver)
            {
                RestartToMenu();
            }
        }

        public void RequestLaneChange(int direction)
        {
            if (state != RunnerState.Running || player == null)
            {
                return;
            }

            player.ChangeLane(direction);
        }

        public void RequestPrimaryAction()
        {
            if (state == RunnerState.Running && player != null)
            {
                player.Jump();
                return;
            }

            if (state == RunnerState.Clash)
            {
                AddClashTap();
            }
        }

        public void AddDistance(float meters)
        {
            if (score != null)
            {
                score.AddDistance(meters);
            }
        }

        public void HandleInteractable(RunnerInteractable interactable, PlayerRunnerController sourcePlayer)
        {
            if (interactable == null || sourcePlayer == null || state == RunnerState.GameOver)
            {
                return;
            }

            if (interactable.type == RunnerInteractableType.Hazard && sourcePlayer.ClearsHeight(interactable.jumpClearHeight))
            {
                return;
            }

            switch (interactable.type)
            {
                case RunnerInteractableType.Person:
                    ConsumeAndScore(interactable, config.personScore);
                    interactable.gameObject.SetActive(false);
                    break;
                case RunnerInteractableType.Destructible:
                    ConsumeAndScore(interactable, config.destructibleScore);
                    BreakDestructible(interactable);
                    break;
                case RunnerInteractableType.Hazard:
                    HandleHazard(interactable, sourcePlayer);
                    break;
                case RunnerInteractableType.PowerUp:
                    HandlePowerUp(interactable);
                    break;
                case RunnerInteractableType.ClashTrigger:
                    ClashController clash = interactable.GetComponentInParent<ClashController>();
                    if (clash != null)
                    {
                        BeginClash(clash, interactable);
                    }
                    break;
                case RunnerInteractableType.RampLanding:
                    ConsumeAndScore(interactable, config.rampLandingScore);
                    break;
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
                worldGenerator.StopGeneration();
            }

            if (player != null)
            {
                player.SetControlsLocked(true);
            }

            if (ui != null && score != null)
            {
                ui.ShowGameOver(score.TotalScore);
            }
        }

        private IEnumerator StartGameRoutine()
        {
            state = RunnerState.TransitionToRun;

            if (ui != null)
            {
                ui.ShowRunning();
            }

            if (player != null)
            {
                player.ResetPlayer();
                player.SetControlsLocked(true);
            }

            if (score != null)
            {
                score.ResetScore();
            }

            if (worldGenerator != null)
            {
                worldGenerator.BeginGeneration();
            }

            if (cameraController != null)
            {
                cameraController.TransitionToRunner(1.05f);
            }

            yield return new WaitForSeconds(1.05f);

            state = RunnerState.Running;
            if (player != null)
            {
                player.SetControlsLocked(false);
            }
        }

        private void HandleHazard(RunnerInteractable interactable, PlayerRunnerController sourcePlayer)
        {
            if (sourcePlayer.IsHazardInvulnerable)
            {
                ConsumeAndScore(interactable, config.poweredHazardScore);
                BreakDestructible(interactable);
                return;
            }

            GameOver();
        }

        private void HandlePowerUp(RunnerInteractable interactable)
        {
            PowerUpPickup pickup = interactable.GetComponent<PowerUpPickup>();
            PowerUpDefinition definition = pickup != null ? pickup.definition : null;
            interactable.Consume();
            interactable.gameObject.SetActive(false);

            if (definition == null)
            {
                return;
            }

            if (definition.bonusScore > 0 && score != null)
            {
                score.AddBonus(definition.bonusScore);
            }

            if (definition.type == PowerUpType.DestroyAll)
            {
                if (destroyAllRoutine != null)
                {
                    StopCoroutine(destroyAllRoutine);
                }
                destroyAllRoutine = StartCoroutine(DestroyAllRoutine(definition.duration));
            }
            else if (definition.type == PowerUpType.DivineRamp)
            {
                if (worldGenerator != null)
                {
                    worldGenerator.ScheduleFallingBlock();
                }

                if (player != null)
                {
                    SpawnDivineRampVisual();
                    player.BeginRampFlight(config.rampFlightDuration, config.rampFlightHeight, config.rampInvulnerabilityPadding);
                }
            }
        }

        private IEnumerator DestroyAllRoutine(float duration)
        {
            if (player != null)
            {
                player.SetHazardInvulnerable(true);
            }

            yield return new WaitForSeconds(duration);

            if (player != null)
            {
                player.SetHazardInvulnerable(false);
            }

            destroyAllRoutine = null;
        }

        private void BeginClash(ClashController clash, RunnerInteractable trigger)
        {
            if (state != RunnerState.Running)
            {
                return;
            }

            trigger.Consume();
            state = RunnerState.Clash;
            activeClash = clash;
            clashPower = 0f;

            if (worldGenerator != null)
            {
                worldGenerator.StopGeneration();
            }

            if (player != null)
            {
                player.SetControlsLocked(true);
                player.SetHazardInvulnerable(true);
            }

            if (cameraController != null)
            {
                cameraController.BeginClashCamera();
            }

            if (ui != null)
            {
                ui.ShowClash();
            }

            activeClash.BeginClash();
        }

        private void AddClashTap()
        {
            if (config == null)
            {
                return;
            }

            clashPower += config.clashTapPower;
            clashPower = Mathf.Clamp(clashPower, 0f, config.clashRequiredPower);
        }

        private void UpdateClash()
        {
            if (activeClash == null || config == null)
            {
                CompleteClash();
                return;
            }

            clashPower = Mathf.Max(0f, clashPower - config.clashPowerDecayPerSecond * Time.deltaTime);
            float progress = Mathf.Clamp01(clashPower / Mathf.Max(0.01f, config.clashRequiredPower));
            activeClash.SetProgress(progress);

            if (cameraController != null)
            {
                cameraController.SetClashProgress(progress);
            }

            if (ui != null)
            {
                ui.SetClashProgress(progress);
            }

            if (progress >= 1f)
            {
                CompleteClash();
            }
        }

        private void CompleteClash()
        {
            if (state != RunnerState.Clash)
            {
                return;
            }

            if (activeClash != null)
            {
                activeClash.CompleteClash();
            }

            if (score != null)
            {
                score.AddBonus(config.clashScore);
            }

            if (worldGenerator != null)
            {
                worldGenerator.AdvanceDifficultyAfterClash();
            }

            StartCoroutine(ResumeAfterClashRoutine());
        }

        private IEnumerator ResumeAfterClashRoutine()
        {
            state = RunnerState.TransitionToRun;
            yield return new WaitForSeconds(0.35f);

            if (cameraController != null)
            {
                cameraController.EndClashCamera(0.65f);
            }

            yield return new WaitForSeconds(0.65f);

            if (player != null)
            {
                player.SetControlsLocked(false);
                player.SetHazardInvulnerable(false);
            }

            if (worldGenerator != null)
            {
                worldGenerator.ResumeGeneration();
            }

            if (ui != null)
            {
                ui.ShowRunning();
            }

            activeClash = null;
            clashPower = 0f;
            state = RunnerState.Running;
        }

        private void ConsumeAndScore(RunnerInteractable interactable, int defaultScore)
        {
            interactable.Consume();
            int points = interactable.scoreOverride != 0 ? interactable.scoreOverride : defaultScore;
            if (score != null)
            {
                score.AddBonus(points);
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

        private void SpawnDivineRampVisual()
        {
            if (player == null)
            {
                return;
            }

            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Divine Ramp Visual";
            ramp.transform.position = new Vector3(player.transform.position.x, 0.28f, player.transform.position.z + 2.4f);
            ramp.transform.rotation = Quaternion.Euler(-18f, 0f, 0f);
            ramp.transform.localScale = new Vector3(2.6f, 0.35f, 4.2f);

            Collider rampCollider = ramp.GetComponent<Collider>();
            if (rampCollider != null)
            {
                Destroy(rampCollider);
            }

            Renderer renderer = ramp.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                Material material = new Material(shader);
                material.color = new Color(0.15f, 0.74f, 0.95f, 0.85f);
                renderer.material = material;
            }

            Destroy(ramp, 2f);
        }

        private void BindReferences()
        {
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerRunnerController>();
            }

            if (worldGenerator == null)
            {
                worldGenerator = FindFirstObjectByType<WorldGenerator>();
            }

            if (score == null)
            {
                score = GetComponent<RunnerScore>();
            }

            if (score == null)
            {
                score = gameObject.AddComponent<RunnerScore>();
            }

            if (inputReader == null)
            {
                inputReader = GetComponent<RunnerInputReader>();
            }

            if (inputReader == null)
            {
                inputReader = gameObject.AddComponent<RunnerInputReader>();
            }

            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<RunnerCameraController>();
            }

            if (ui == null)
            {
                ui = GetComponent<RunnerUI>();
            }

            if (ui == null)
            {
                ui = gameObject.AddComponent<RunnerUI>();
            }

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
            }

            if (cameraController != null && player != null)
            {
                cameraController.player = player.transform;
            }
        }
    }
}
