using System.Collections.Generic;
using UnityEngine;

namespace InfinityRunner
{
    public static class RuntimePrototypeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototypeIfNeeded()
        {
            if (GameCoordinator.Instance != null || Object.FindFirstObjectByType<GameCoordinator>() != null)
            {
                return;
            }

            RuntimePrototypeFactory.Create();
        }
    }

    internal static class RuntimePrototypeFactory
    {
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        public static void Create()
        {
            Materials.Clear();
            CreateMaterials();

            RunnerConfig config = CreateRunnerConfig();
            DifficultyStageConfig[] stages = CreateStages();

            Transform templateRoot = new GameObject("Runtime Block Templates").transform;
            templateRoot.gameObject.SetActive(false);

            PowerUpDefinition[] powerUps = CreatePowerUps(templateRoot);
            BlockDefinition[] blocks = CreateBlocks(templateRoot);

            EnsureLighting();
            Camera camera = EnsureCamera();
            PlayerRunnerController player = CreatePlayer(config);

            GameObject coordinatorObject = new GameObject("Game Coordinator");
            coordinatorObject.SetActive(false);
            GameCoordinator coordinator = coordinatorObject.AddComponent<GameCoordinator>();
            RunnerScore score = coordinatorObject.AddComponent<RunnerScore>();
            RunnerInputReader input = coordinatorObject.AddComponent<RunnerInputReader>();
            RunnerUI ui = coordinatorObject.AddComponent<RunnerUI>();

            GameObject generatorObject = new GameObject("World Generator");
            WorldGenerator generator = generatorObject.AddComponent<WorldGenerator>();
            GameObject worldRoot = new GameObject("Generated World");
            worldRoot.transform.SetParent(generatorObject.transform, false);
            generator.worldRoot = worldRoot.transform;
            generator.config = config;
            generator.difficultyStages = stages;
            generator.blockDefinitions = blocks;
            generator.powerUps = powerUps;

            RunnerCameraController cameraController = camera.gameObject.AddComponent<RunnerCameraController>();
            cameraController.targetCamera = camera;
            cameraController.player = player.transform;

            coordinator.config = config;
            coordinator.player = player;
            coordinator.worldGenerator = generator;
            coordinator.score = score;
            coordinator.inputReader = input;
            coordinator.cameraController = cameraController;
            coordinator.ui = ui;

            score.config = config;
            input.config = config;
            input.coordinator = coordinator;
            ui.coordinator = coordinator;

            coordinatorObject.SetActive(true);
        }

        private static void CreateMaterials()
        {
            Materials["Ground"] = CreateMaterial(new Color(0.39f, 0.32f, 0.27f));
            Materials["Lane"] = CreateMaterial(new Color(0.88f, 0.82f, 0.62f));
            Materials["Rock"] = CreateMaterial(new Color(0.27f, 0.28f, 0.28f));
            Materials["Person"] = CreateMaterial(new Color(0.18f, 0.65f, 0.85f));
            Materials["Prop"] = CreateMaterial(new Color(0.55f, 0.18f, 0.09f));
            Materials["Hazard"] = CreateMaterial(new Color(0.12f, 0.08f, 0.06f));
            Materials["Wall"] = CreateMaterial(new Color(0.48f, 0.45f, 0.4f));
            Materials["Warning"] = CreateMaterial(new Color(0.9f, 0.05f, 0.02f));
            Materials["Gold"] = CreateMaterial(new Color(1f, 0.72f, 0.12f));
            Materials["Divine"] = CreateMaterial(new Color(0.15f, 0.74f, 0.95f));
            Materials["Grass"] = CreateMaterial(new Color(0.18f, 0.46f, 0.2f));
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private static RunnerConfig CreateRunnerConfig()
        {
            RunnerConfig config = ScriptableObject.CreateInstance<RunnerConfig>();
            config.laneSpacing = 3f;
            config.laneChangeDuration = 0.18f;
            config.jumpVelocity = 12.5f;
            config.gravity = 28f;
            config.groundHeight = 0.75f;
            config.jumpClearHeight = 1.25f;
            config.spawnAheadDistance = 130f;
            config.despawnBehindDistance = 45f;
            config.initialBlocks = 4;
            config.maxBlocksActive = 7;
            config.touchSwipeThresholdPixels = 80f;
            config.touchTapMaxDuration = 0.25f;
            config.distanceScorePerMeter = 1f;
            config.personScore = 100;
            config.destructibleScore = 25;
            config.poweredHazardScore = 75;
            config.clashScore = 500;
            config.rampLandingScore = 200;
            config.powerUpChancePerEligibleBlock = 0.35f;
            config.powerUpCooldownBlocks = 3;
            config.clashRequiredPower = 8f;
            config.clashTapPower = 1f;
            config.clashPowerDecayPerSecond = 0.25f;
            config.rampFlightDuration = 1.55f;
            config.rampFlightHeight = 7f;
            config.rampInvulnerabilityPadding = 0.35f;
            return config;
        }

        private static DifficultyStageConfig[] CreateStages()
        {
            DifficultyStageConfig start = ScriptableObject.CreateInstance<DifficultyStageConfig>();
            start.stage = DifficultyStage.Start;
            start.speedMetersPerSecond = 10f;
            start.blocksBeforeForcedClash = 8;

            DifficultyStageConfig middle = ScriptableObject.CreateInstance<DifficultyStageConfig>();
            middle.stage = DifficultyStage.Middle;
            middle.speedMetersPerSecond = 16f;
            middle.blocksBeforeForcedClash = 9;

            DifficultyStageConfig late = ScriptableObject.CreateInstance<DifficultyStageConfig>();
            late.stage = DifficultyStage.Late;
            late.speedMetersPerSecond = 22f;
            late.blocksBeforeForcedClash = 10;
            late.lateClashChance = 0.08f;
            late.minimumBlocksBetweenLateClashes = 9;

            return new[] { start, middle, late };
        }

        private static PowerUpDefinition[] CreatePowerUps(Transform templateRoot)
        {
            GameObject destroyPrefab = CreatePowerUpTemplate("PU_DestroyAll", Materials["Gold"], templateRoot);
            GameObject rampPrefab = CreatePowerUpTemplate("PU_DivineRamp", Materials["Divine"], templateRoot);

            PowerUpDefinition destroyAll = ScriptableObject.CreateInstance<PowerUpDefinition>();
            destroyAll.type = PowerUpType.DestroyAll;
            destroyAll.pickupPrefab = destroyPrefab;
            destroyAll.duration = 5f;

            PowerUpDefinition divineRamp = ScriptableObject.CreateInstance<PowerUpDefinition>();
            divineRamp.type = PowerUpType.DivineRamp;
            divineRamp.pickupPrefab = rampPrefab;
            divineRamp.duration = 1.55f;
            divineRamp.requiresFallingBlockAfterPickup = true;

            return new[] { destroyAll, divineRamp };
        }

        private static BlockDefinition[] CreateBlocks(Transform templateRoot)
        {
            return new[]
            {
                CreateBlockDefinition("Safe", CreateSafeBlock("Block_Safe", BlockKind.Safe, templateRoot), BlockKind.Safe, DifficultyStageMask.All, 4, LaneMask.All, LaneMask.None, false),
                CreateBlockDefinition("Props", CreatePropsBlock(templateRoot), BlockKind.PropsAndPeople, DifficultyStageMask.All, 5, LaneMask.All, LaneMask.None, false),
                CreateBlockDefinition("Jump", CreateJumpBlock(templateRoot), BlockKind.Jump, DifficultyStageMask.All, 3, LaneMask.All, LaneMask.All, false),
                CreateBlockDefinition("Lane", CreateLaneBlock(templateRoot), BlockKind.LaneBlock, DifficultyStageMask.All, 3, LaneMask.Right, LaneMask.Left | LaneMask.Center, false),
                CreateBlockDefinition("Cart", CreateCartBlock(templateRoot), BlockKind.DynamicCart, DifficultyStageMask.All, 2, LaneMask.All, LaneMask.Center, false),
                CreateBlockDefinition("Pillar", CreatePillarBlock(templateRoot), BlockKind.FallingPillar, DifficultyStageMask.Middle | DifficultyStageMask.Late, 2, LaneMask.Center, LaneMask.Left | LaneMask.Right, false),
                CreateBlockDefinition("Catapult", CreateCatapultBlock(templateRoot), BlockKind.Catapult, DifficultyStageMask.Middle | DifficultyStageMask.Late, 2, LaneMask.Left | LaneMask.Right, LaneMask.Center, false),
                CreateBlockDefinition("FallingBlock", CreateFallingBlock(templateRoot), BlockKind.FallingBlock, DifficultyStageMask.All, 1, LaneMask.All, LaneMask.None, true),
                CreateBlockDefinition("Clash", CreateClashBlock(templateRoot), BlockKind.Clash, DifficultyStageMask.All, 1, LaneMask.All, LaneMask.All, true)
            };
        }

        private static BlockDefinition CreateBlockDefinition(string name, GameObject prefab, BlockKind kind, DifficultyStageMask stages, int weight, LaneMask safeLanes, LaneMask occupiedLanes, bool isSpecial)
        {
            BlockDefinition definition = ScriptableObject.CreateInstance<BlockDefinition>();
            definition.name = name;
            definition.prefab = prefab;
            definition.length = 30f;
            definition.kind = kind;
            definition.allowedStages = stages;
            definition.weight = weight;
            definition.safeLanes = safeLanes;
            definition.occupiedLanes = occupiedLanes;
            definition.isSpecial = isSpecial;
            return definition;
        }

        private static GameObject CreatePowerUpTemplate(string name, Material material, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            GameObject visual = CreatePrimitive(PrimitiveType.Sphere, "Visual", root.transform, Vector3.zero, new Vector3(0.9f, 0.9f, 0.9f), material);
            Object.Destroy(visual.GetComponent<Collider>());

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.6f;
            RunnerInteractable interactable = root.AddComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.PowerUp;
            root.AddComponent<PowerUpPickup>();
            return root;
        }

        private static GameObject CreateSafeBlock(string name, BlockKind kind, Transform parent)
        {
            GameObject root = CreateBaseBlock(name, kind, parent);
            AddSideDecor(root.transform);
            AddPowerUpSpawn(root.transform, Lane.Left, 10f, true, false);
            AddPowerUpSpawn(root.transform, Lane.Right, 20f, true, true);
            return root;
        }

        private static GameObject CreatePropsBlock(Transform parent)
        {
            GameObject root = CreateBaseBlock("Block_PropsPeople", BlockKind.PropsAndPeople, parent);
            AddDestructibleBox(root.transform, Lane.Left, 8f, "Crates L");
            AddDestructibleBox(root.transform, Lane.Center, 15f, "Barrels C");
            AddDestructibleBox(root.transform, Lane.Right, 23f, "Market R");
            AddHuman(root.transform, Lane.Center, 7f);
            AddHuman(root.transform, Lane.Right, 12f);
            AddHuman(root.transform, Lane.Left, 21f);
            AddPowerUpSpawn(root.transform, Lane.Center, 18f, true, true);
            AddSideDecor(root.transform);
            return root;
        }

        private static GameObject CreateJumpBlock(Transform parent)
        {
            GameObject root = CreateBaseBlock("Block_JumpTrap", BlockKind.Jump, parent);
            GameObject trap = CreatePrimitive(PrimitiveType.Cube, "Low Trap Hazard", root.transform, new Vector3(0f, 0.35f, 16f), new Vector3(9.4f, 0.7f, 1.4f), Materials["Hazard"]);
            trap.GetComponent<Collider>().isTrigger = true;
            RunnerInteractable interactable = trap.AddComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.Hazard;
            interactable.jumpClearHeight = 1.15f;
            GameObject pit = CreatePrimitive(PrimitiveType.Cube, "Pit Visual", root.transform, new Vector3(0f, 0.02f, 16f), new Vector3(9.8f, 0.05f, 3.4f), Materials["Hazard"]);
            Object.Destroy(pit.GetComponent<Collider>());
            AddPowerUpSpawn(root.transform, Lane.Right, 23f, true, false);
            AddSideDecor(root.transform);
            return root;
        }

        private static GameObject CreateLaneBlock(Transform parent)
        {
            GameObject root = CreateBaseBlock("Block_LaneWalls", BlockKind.LaneBlock, parent);
            AddWallHazard(root.transform, Lane.Left, 15f, "Wall Left");
            AddWallHazard(root.transform, Lane.Center, 15f, "Wall Center");
            AddHuman(root.transform, Lane.Right, 12f);
            AddPowerUpSpawn(root.transform, Lane.Right, 21f, true, false);
            AddSideDecor(root.transform);
            return root;
        }

        private static GameObject CreateCartBlock(Transform parent)
        {
            GameObject root = CreateBaseBlock("Block_DynamicCart", BlockKind.DynamicCart, parent);
            GameObject cartRoot = new GameObject("Dynamic Cart Root");
            cartRoot.transform.SetParent(root.transform, false);
            cartRoot.transform.localPosition = new Vector3(-3f, 0f, 17f);

            GameObject cart = CreatePrimitive(PrimitiveType.Cube, "Cart Hazard", cartRoot.transform, new Vector3(0f, 0.8f, 0f), new Vector3(2.1f, 1.4f, 2.2f), Materials["Prop"]);
            cart.GetComponent<Collider>().isTrigger = true;
            RunnerInteractable interactable = cart.AddComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.Hazard;
            interactable.jumpClearHeight = 99f;

            GameObject stake = CreatePrimitive(PrimitiveType.Cylinder, "Stake", cartRoot.transform, new Vector3(0f, 1.35f, -1.4f), new Vector3(0.25f, 1.8f, 0.25f), Materials["Hazard"]);
            stake.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Object.Destroy(stake.GetComponent<Collider>());

            DynamicCartObstacle dynamicCart = root.AddComponent<DynamicCartObstacle>();
            dynamicCart.movingRoot = cartRoot.transform;
            dynamicCart.telegraphRenderer = cart.GetComponent<Renderer>();

            AddHuman(root.transform, Lane.Left, 13f);
            AddHuman(root.transform, Lane.Left, 20f);
            AddSideDecor(root.transform);
            return root;
        }

        private static GameObject CreatePillarBlock(Transform parent)
        {
            GameObject root = CreateBaseBlock("Block_FallingPillar", BlockKind.FallingPillar, parent);
            GameObject pillarRoot = new GameObject("Falling Pillar");
            pillarRoot.transform.SetParent(root.transform, false);
            pillarRoot.transform.localPosition = new Vector3(0f, 0f, 17f);
            GameObject visual = CreatePrimitive(PrimitiveType.Cube, "Pillar Visual", pillarRoot.transform, new Vector3(0f, 2.2f, 0f), new Vector3(1.2f, 4.4f, 1.2f), Materials["Wall"]);
            Collider vertical = visual.GetComponent<Collider>();
            vertical.isTrigger = true;
            RunnerInteractable verticalInteractable = visual.AddComponent<RunnerInteractable>();
            verticalInteractable.type = RunnerInteractableType.Hazard;
            verticalInteractable.jumpClearHeight = 99f;

            GameObject fallen = CreatePrimitive(PrimitiveType.Cube, "Fallen Hazard", pillarRoot.transform, new Vector3(0f, 0.8f, 0f), new Vector3(9f, 1.3f, 1.2f), Materials["Wall"]);
            Collider fallenCollider = fallen.GetComponent<Collider>();
            fallenCollider.isTrigger = true;
            fallenCollider.enabled = false;
            RunnerInteractable fallenInteractable = fallen.AddComponent<RunnerInteractable>();
            fallenInteractable.type = RunnerInteractableType.Hazard;
            fallenInteractable.jumpClearHeight = 99f;

            FallingPillarObstacle pillar = root.AddComponent<FallingPillarObstacle>();
            pillar.pillarVisual = visual.transform;
            pillar.verticalHazard = vertical;
            pillar.fallenHazard = fallenCollider;
            AddPowerUpSpawn(root.transform, Lane.Center, 10f, true, false);
            AddSideDecor(root.transform);
            return root;
        }

        private static GameObject CreateCatapultBlock(Transform parent)
        {
            GameObject projectilePrefab = CreateCatapultProjectile(parent);
            GameObject root = CreateBaseBlock("Block_Catapult", BlockKind.Catapult, parent);
            GameObject warning = CreatePrimitive(PrimitiveType.Cube, "Warning Strip", root.transform, new Vector3(0f, 0.04f, 15f), new Vector3(2.4f, 0.04f, 18f), Materials["Warning"]);
            Object.Destroy(warning.GetComponent<Collider>());
            warning.GetComponent<Renderer>().enabled = false;

            GameObject catapultRoot = new GameObject("Catapult");
            catapultRoot.transform.SetParent(root.transform, false);
            catapultRoot.transform.localPosition = new Vector3(0f, 0f, 27f);
            CreatePrimitive(PrimitiveType.Cube, "Catapult Base", catapultRoot.transform, new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 1.8f), Materials["Prop"]);
            GameObject arm = CreatePrimitive(PrimitiveType.Cube, "Arm", catapultRoot.transform, new Vector3(0f, 1.35f, -0.4f), new Vector3(0.35f, 0.35f, 3.2f), Materials["Wall"]);
            Object.Destroy(arm.GetComponent<Collider>());

            GameObject staticHazard = CreatePrimitive(PrimitiveType.Cube, "Catapult Body Hazard", root.transform, new Vector3(0f, 1f, 27f), new Vector3(2.7f, 2f, 2.3f), Materials["Wall"]);
            staticHazard.GetComponent<Collider>().isTrigger = true;
            RunnerInteractable hazard = staticHazard.AddComponent<RunnerInteractable>();
            hazard.type = RunnerInteractableType.Hazard;
            hazard.jumpClearHeight = 99f;

            CatapultObstacle catapult = root.AddComponent<CatapultObstacle>();
            catapult.arm = arm.transform;
            catapult.firePoint = catapultRoot.transform;
            catapult.projectilePrefab = projectilePrefab.GetComponent<RunnerInteractable>();
            catapult.warningRenderer = warning.GetComponent<Renderer>();

            AddPowerUpSpawn(root.transform, Lane.Left, 12f, true, false);
            AddPowerUpSpawn(root.transform, Lane.Right, 18f, true, false);
            AddSideDecor(root.transform);
            return root;
        }

        private static GameObject CreateCatapultProjectile(Transform parent)
        {
            GameObject root = new GameObject("Catapult_Projectile");
            root.transform.SetParent(parent, false);
            GameObject visual = CreatePrimitive(PrimitiveType.Sphere, "Visual", root.transform, Vector3.zero, new Vector3(1f, 1f, 1f), Materials["Hazard"]);
            Object.Destroy(visual.GetComponent<Collider>());
            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.55f;
            RunnerInteractable interactable = root.AddComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.Hazard;
            interactable.jumpClearHeight = 99f;
            return root;
        }

        private static GameObject CreateFallingBlock(Transform parent)
        {
            GameObject root = CreateBaseBlock("Block_FallingBlock", BlockKind.FallingBlock, parent);
            GameObject marker = CreatePrimitive(PrimitiveType.Cylinder, "Divine Landing Marker", root.transform, new Vector3(0f, 0.05f, 15f), new Vector3(4.5f, 0.08f, 4.5f), Materials["Divine"]);
            marker.GetComponent<Collider>().isTrigger = true;
            RunnerInteractable interactable = marker.AddComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.RampLanding;
            interactable.jumpClearHeight = 99f;
            AddSideDecor(root.transform);
            return root;
        }

        private static GameObject CreateClashBlock(Transform parent)
        {
            GameObject root = CreateBaseBlock("Block_Clash", BlockKind.Clash, parent);
            ClashController clash = root.AddComponent<ClashController>();
            GameObject wallRoot = new GameObject("Clash Wall Root");
            wallRoot.transform.SetParent(root.transform, false);
            wallRoot.transform.localPosition = new Vector3(0f, 0f, 18f);
            CreatePrimitive(PrimitiveType.Cube, "Main Wall", wallRoot.transform, new Vector3(0f, 2.2f, 0f), new Vector3(10.5f, 4.4f, 1.2f), Materials["Wall"]);
            CreatePrimitive(PrimitiveType.Cube, "Wall Cap", wallRoot.transform, new Vector3(0f, 4.65f, 0f), new Vector3(11f, 0.5f, 1.4f), Materials["Hazard"]);

            GameObject trigger = CreatePrimitive(PrimitiveType.Cube, "Clash Trigger", root.transform, new Vector3(0f, 1.4f, 5f), new Vector3(10f, 2.8f, 1f), Materials["Warning"]);
            trigger.GetComponent<Renderer>().enabled = false;
            trigger.GetComponent<Collider>().isTrigger = true;
            RunnerInteractable triggerInteractable = trigger.AddComponent<RunnerInteractable>();
            triggerInteractable.type = RunnerInteractableType.ClashTrigger;
            triggerInteractable.jumpClearHeight = 99f;
            clash.wallRoot = wallRoot.transform;
            AddSideDecor(root.transform);
            return root;
        }

        private static GameObject CreateBaseBlock(string name, BlockKind kind, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            BlockMetadata metadata = root.AddComponent<BlockMetadata>();
            metadata.length = 30f;
            metadata.kind = kind;
            CreatePrimitive(PrimitiveType.Cube, "Ground", root.transform, new Vector3(0f, -0.1f, 15f), new Vector3(10.5f, 0.2f, 30f), Materials["Ground"]);
            CreatePrimitive(PrimitiveType.Cube, "Left Verge", root.transform, new Vector3(-6f, 0.02f, 15f), new Vector3(1.5f, 0.05f, 30f), Materials["Grass"]);
            CreatePrimitive(PrimitiveType.Cube, "Right Verge", root.transform, new Vector3(6f, 0.02f, 15f), new Vector3(1.5f, 0.05f, 30f), Materials["Grass"]);
            CreatePrimitive(PrimitiveType.Cube, "Lane Line L", root.transform, new Vector3(-1.5f, 0.03f, 15f), new Vector3(0.08f, 0.05f, 30f), Materials["Lane"]);
            CreatePrimitive(PrimitiveType.Cube, "Lane Line R", root.transform, new Vector3(1.5f, 0.03f, 15f), new Vector3(0.08f, 0.05f, 30f), Materials["Lane"]);
            return root;
        }

        private static void AddSideDecor(Transform root)
        {
            for (int i = 0; i < 3; i++)
            {
                float z = 6f + i * 9f;
                CreatePrimitive(PrimitiveType.Cube, "House L " + i, root, new Vector3(-8.2f, 1.1f, z), new Vector3(2.2f, 2.2f, 2.2f), Materials["Wall"]);
                CreatePrimitive(PrimitiveType.Cube, "House R " + i, root, new Vector3(8.2f, 1.1f, z + 3f), new Vector3(2.2f, 2.2f, 2.2f), Materials["Wall"]);
            }
        }

        private static void AddPowerUpSpawn(Transform root, Lane lane, float z, bool allowDestroyAll, bool allowDivineRamp)
        {
            GameObject point = new GameObject("PowerUpSpawn_" + lane + "_" + z.ToString("0"));
            point.transform.SetParent(root, false);
            point.transform.localPosition = new Vector3((int)lane * 3f, 1.2f, z);
            PowerUpSpawnPoint spawnPoint = point.AddComponent<PowerUpSpawnPoint>();
            spawnPoint.lane = lane;
            spawnPoint.allowDestroyAll = allowDestroyAll;
            spawnPoint.allowDivineRamp = allowDivineRamp;
        }

        private static void AddHuman(Transform root, Lane lane, float z)
        {
            GameObject human = CreatePrimitive(PrimitiveType.Capsule, "Human " + lane + " " + z.ToString("0"), root, new Vector3((int)lane * 3f, 0.85f, z), new Vector3(0.65f, 0.85f, 0.65f), Materials["Person"]);
            human.GetComponent<Collider>().isTrigger = true;
            RunnerInteractable interactable = human.AddComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.Person;
            interactable.jumpClearHeight = 99f;
            human.AddComponent<HumanPickup>();
        }

        private static void AddDestructibleBox(Transform root, Lane lane, float z, string name)
        {
            GameObject prop = CreatePrimitive(PrimitiveType.Cube, name, root, new Vector3((int)lane * 3f, 0.65f, z), new Vector3(1.4f, 1.3f, 1.4f), Materials["Prop"]);
            prop.GetComponent<Collider>().isTrigger = true;
            RunnerInteractable interactable = prop.AddComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.Destructible;
            interactable.jumpClearHeight = 99f;
            prop.AddComponent<DestructibleProp>();
        }

        private static void AddWallHazard(Transform root, Lane lane, float z, string name)
        {
            GameObject wall = CreatePrimitive(PrimitiveType.Cube, name, root, new Vector3((int)lane * 3f, 1.45f, z), new Vector3(2.6f, 2.9f, 2.2f), Materials["Wall"]);
            wall.GetComponent<Collider>().isTrigger = true;
            RunnerInteractable interactable = wall.AddComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.Hazard;
            interactable.jumpClearHeight = 99f;
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject instance = GameObject.CreatePrimitive(type);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = localScale;
            Renderer renderer = instance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
            return instance;
        }

        private static PlayerRunnerController CreatePlayer(RunnerConfig config)
        {
            GameObject player = new GameObject("Player Rock");
            player.transform.position = new Vector3(0f, config.groundHeight, 0f);
            SphereCollider collider = player.AddComponent<SphereCollider>();
            collider.radius = 0.75f;
            Rigidbody body = player.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            GameObject visual = CreatePrimitive(PrimitiveType.Sphere, "Rock Visual", player.transform, Vector3.zero, new Vector3(1.5f, 1.5f, 1.5f), Materials["Rock"]);
            Object.Destroy(visual.GetComponent<Collider>());
            PlayerRunnerController controller = player.AddComponent<PlayerRunnerController>();
            controller.visualRoot = visual.transform;
            controller.config = config;
            return controller;
        }

        private static void EnsureLighting()
        {
            if (Object.FindFirstObjectByType<Light>() != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static Camera EnsureCamera()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                return camera;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 300f;
            camera.fieldOfView = 62f;
            return camera;
        }
    }
}
