using System.Collections.Generic;
using InfinityRunner;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RunnerProjectBuilder
{
    private const string RootPath = "Assets/_Game";
    private const string ConfigPath = RootPath + "/Config";
    private const string PrefabPath = RootPath + "/Prefabs";
    private const string MaterialPath = RootPath + "/Materials";
    private const string ScenePath = RootPath + "/Scenes/RunnerPrototype.unity";

    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    [MenuItem("Infinity Runner/Build Prototype Content")]
    public static void BuildPrototypeContent()
    {
        EnsureFolders();
        Materials.Clear();
        CreateMaterials();

        RunnerConfig runnerConfig = CreateRunnerConfig();
        DifficultyStageConfig[] stages = CreateDifficultyStages();
        PowerUpDefinition[] powerUps = CreatePowerUps();
        BlockDefinition[] blocks = CreateBlocks(powerUps);

        CreateScene(runnerConfig, stages, blocks, powerUps);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Infinity Runner prototype content rebuilt.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "_Game");
        EnsureFolder(RootPath, "Config");
        EnsureFolder(RootPath, "Prefabs");
        EnsureFolder(RootPath, "Materials");
        EnsureFolder(RootPath, "Scenes");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void CreateMaterials()
    {
        Materials["Ground"] = CreateMaterial("M_Ground", new Color(0.39f, 0.32f, 0.27f));
        Materials["Lane"] = CreateMaterial("M_LaneLine", new Color(0.88f, 0.82f, 0.62f));
        Materials["Rock"] = CreateMaterial("M_Rock", new Color(0.27f, 0.28f, 0.28f));
        Materials["Person"] = CreateMaterial("M_Person", new Color(0.18f, 0.65f, 0.85f));
        Materials["Prop"] = CreateMaterial("M_Prop", new Color(0.55f, 0.18f, 0.09f));
        Materials["Hazard"] = CreateMaterial("M_Hazard", new Color(0.12f, 0.08f, 0.06f));
        Materials["Wall"] = CreateMaterial("M_Wall", new Color(0.48f, 0.45f, 0.4f));
        Materials["Warning"] = CreateMaterial("M_Warning", new Color(0.9f, 0.05f, 0.02f));
        Materials["Gold"] = CreateMaterial("M_Gold", new Color(1f, 0.72f, 0.12f));
        Materials["Divine"] = CreateMaterial("M_Divine", new Color(0.15f, 0.74f, 0.95f));
        Materials["Grass"] = CreateMaterial("M_Grass", new Color(0.18f, 0.46f, 0.2f));
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        string path = MaterialPath + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static RunnerConfig CreateRunnerConfig()
    {
        RunnerConfig config = CreateOrLoadAsset<RunnerConfig>(ConfigPath + "/RunnerConfig.asset");
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
        EditorUtility.SetDirty(config);
        return config;
    }

    private static DifficultyStageConfig[] CreateDifficultyStages()
    {
        DifficultyStageConfig start = CreateOrLoadAsset<DifficultyStageConfig>(ConfigPath + "/Difficulty_Start.asset");
        start.stage = DifficultyStage.Start;
        start.speedMetersPerSecond = 10f;
        start.blocksBeforeForcedClash = 8;
        start.lateClashChance = 0f;
        start.minimumBlocksBetweenLateClashes = 8;
        EditorUtility.SetDirty(start);

        DifficultyStageConfig middle = CreateOrLoadAsset<DifficultyStageConfig>(ConfigPath + "/Difficulty_Middle.asset");
        middle.stage = DifficultyStage.Middle;
        middle.speedMetersPerSecond = 16f;
        middle.blocksBeforeForcedClash = 9;
        middle.lateClashChance = 0f;
        middle.minimumBlocksBetweenLateClashes = 8;
        EditorUtility.SetDirty(middle);

        DifficultyStageConfig late = CreateOrLoadAsset<DifficultyStageConfig>(ConfigPath + "/Difficulty_Late.asset");
        late.stage = DifficultyStage.Late;
        late.speedMetersPerSecond = 22f;
        late.blocksBeforeForcedClash = 10;
        late.lateClashChance = 0.08f;
        late.minimumBlocksBetweenLateClashes = 9;
        EditorUtility.SetDirty(late);

        return new[] { start, middle, late };
    }

    private static PowerUpDefinition[] CreatePowerUps()
    {
        GameObject destroyAllPrefab = CreatePowerUpPrefab("PU_DestroyAll", PowerUpType.DestroyAll, Materials["Gold"]);
        GameObject divineRampPrefab = CreatePowerUpPrefab("PU_DivineRamp", PowerUpType.DivineRamp, Materials["Divine"]);

        PowerUpDefinition destroyAll = CreateOrLoadAsset<PowerUpDefinition>(ConfigPath + "/PowerUp_DestroyAll.asset");
        destroyAll.type = PowerUpType.DestroyAll;
        destroyAll.pickupPrefab = destroyAllPrefab;
        destroyAll.duration = 5f;
        destroyAll.bonusScore = 0;
        destroyAll.requiresFallingBlockAfterPickup = false;
        EditorUtility.SetDirty(destroyAll);

        PowerUpDefinition divineRamp = CreateOrLoadAsset<PowerUpDefinition>(ConfigPath + "/PowerUp_DivineRamp.asset");
        divineRamp.type = PowerUpType.DivineRamp;
        divineRamp.pickupPrefab = divineRampPrefab;
        divineRamp.duration = 1.55f;
        divineRamp.bonusScore = 0;
        divineRamp.requiresFallingBlockAfterPickup = true;
        EditorUtility.SetDirty(divineRamp);

        return new[] { destroyAll, divineRamp };
    }

    private static BlockDefinition[] CreateBlocks(PowerUpDefinition[] powerUps)
    {
        GameObject safe = CreateSafeBlockPrefab("Block_Safe", BlockKind.Safe, true);
        GameObject props = CreatePropsBlockPrefab();
        GameObject jump = CreateJumpBlockPrefab();
        GameObject laneBlock = CreateLaneBlockPrefab();
        GameObject cart = CreateCartBlockPrefab();
        GameObject pillar = CreatePillarBlockPrefab();
        GameObject catapult = CreateCatapultBlockPrefab();
        GameObject falling = CreateFallingBlockPrefab();
        GameObject clash = CreateClashBlockPrefab();

        List<BlockDefinition> definitions = new List<BlockDefinition>();
        definitions.Add(CreateBlockDefinition("BD_Safe", safe, BlockKind.Safe, DifficultyStageMask.All, 4, LaneMask.All, LaneMask.None, false));
        definitions.Add(CreateBlockDefinition("BD_PropsPeople", props, BlockKind.PropsAndPeople, DifficultyStageMask.All, 5, LaneMask.All, LaneMask.None, false));
        definitions.Add(CreateBlockDefinition("BD_JumpTrap", jump, BlockKind.Jump, DifficultyStageMask.All, 3, LaneMask.All, LaneMask.All, false));
        definitions.Add(CreateBlockDefinition("BD_LaneWalls", laneBlock, BlockKind.LaneBlock, DifficultyStageMask.Start | DifficultyStageMask.Middle | DifficultyStageMask.Late, 3, LaneMask.Right, LaneMask.Left | LaneMask.Center, false));
        definitions.Add(CreateBlockDefinition("BD_DynamicCart", cart, BlockKind.DynamicCart, DifficultyStageMask.Start | DifficultyStageMask.Middle | DifficultyStageMask.Late, 2, LaneMask.All, LaneMask.Center, false));
        definitions.Add(CreateBlockDefinition("BD_FallingPillar", pillar, BlockKind.FallingPillar, DifficultyStageMask.Middle | DifficultyStageMask.Late, 2, LaneMask.Center, LaneMask.Left | LaneMask.Right, false));
        definitions.Add(CreateBlockDefinition("BD_Catapult", catapult, BlockKind.Catapult, DifficultyStageMask.Middle | DifficultyStageMask.Late, 2, LaneMask.Left | LaneMask.Right, LaneMask.Center, false));
        definitions.Add(CreateBlockDefinition("BD_FallingBlock", falling, BlockKind.FallingBlock, DifficultyStageMask.All, 1, LaneMask.All, LaneMask.None, true));
        definitions.Add(CreateBlockDefinition("BD_Clash", clash, BlockKind.Clash, DifficultyStageMask.All, 1, LaneMask.All, LaneMask.All, true));
        return definitions.ToArray();
    }

    private static BlockDefinition CreateBlockDefinition(string assetName, GameObject prefab, BlockKind kind, DifficultyStageMask stages, int weight, LaneMask safeLanes, LaneMask occupiedLanes, bool isSpecial)
    {
        BlockDefinition definition = CreateOrLoadAsset<BlockDefinition>(ConfigPath + "/" + assetName + ".asset");
        definition.prefab = prefab;
        definition.length = 30f;
        definition.kind = kind;
        definition.allowedStages = stages;
        definition.weight = weight;
        definition.safeLanes = safeLanes;
        definition.occupiedLanes = occupiedLanes;
        definition.isSpecial = isSpecial;
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static GameObject CreatePowerUpPrefab(string prefabName, PowerUpType type, Material material)
    {
        GameObject root = new GameObject(prefabName);
        GameObject visual = CreatePrimitive(PrimitiveType.Sphere, "Visual", root.transform, Vector3.zero, new Vector3(0.9f, 0.9f, 0.9f), material);
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        SphereCollider collider = root.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.6f;

        RunnerInteractable interactable = root.AddComponent<RunnerInteractable>();
        interactable.type = RunnerInteractableType.PowerUp;
        PowerUpPickup pickup = root.AddComponent<PowerUpPickup>();
        pickup.definition = null;

        return SavePrefab(root, PrefabPath + "/" + prefabName + ".prefab");
    }

    private static GameObject CreateSafeBlockPrefab(string prefabName, BlockKind kind, bool includePowerUpPoints)
    {
        GameObject root = CreateBaseBlock(prefabName, kind);
        AddSideDecor(root.transform);
        if (includePowerUpPoints)
        {
            AddPowerUpSpawn(root.transform, Lane.Left, 10f, true, false);
            AddPowerUpSpawn(root.transform, Lane.Right, 20f, true, true);
        }
        return SavePrefab(root, PrefabPath + "/" + prefabName + ".prefab");
    }

    private static GameObject CreatePropsBlockPrefab()
    {
        GameObject root = CreateBaseBlock("Block_PropsPeople", BlockKind.PropsAndPeople);
        AddDestructibleBox(root.transform, Lane.Left, 8f, "Crates L");
        AddDestructibleBox(root.transform, Lane.Center, 15f, "Barrels C");
        AddDestructibleBox(root.transform, Lane.Right, 23f, "Market R");
        AddHuman(root.transform, Lane.Center, 7f);
        AddHuman(root.transform, Lane.Right, 12f);
        AddHuman(root.transform, Lane.Left, 21f);
        AddPowerUpSpawn(root.transform, Lane.Center, 18f, true, true);
        AddSideDecor(root.transform);
        return SavePrefab(root, PrefabPath + "/Block_PropsPeople.prefab");
    }

    private static GameObject CreateJumpBlockPrefab()
    {
        GameObject root = CreateBaseBlock("Block_JumpTrap", BlockKind.Jump);
        GameObject trap = CreatePrimitive(PrimitiveType.Cube, "Low Trap Hazard", root.transform, new Vector3(0f, 0.35f, 16f), new Vector3(9.4f, 0.7f, 1.4f), Materials["Hazard"]);
        Collider trapCollider = trap.GetComponent<Collider>();
        trapCollider.isTrigger = true;
        RunnerInteractable interactable = trap.AddComponent<RunnerInteractable>();
        interactable.type = RunnerInteractableType.Hazard;
        interactable.jumpClearHeight = 1.15f;

        GameObject pit = CreatePrimitive(PrimitiveType.Cube, "Pit Visual", root.transform, new Vector3(0f, 0.02f, 16f), new Vector3(9.8f, 0.05f, 3.4f), Materials["Hazard"]);
        Object.DestroyImmediate(pit.GetComponent<Collider>());
        AddPowerUpSpawn(root.transform, Lane.Right, 23f, true, false);
        AddSideDecor(root.transform);
        return SavePrefab(root, PrefabPath + "/Block_JumpTrap.prefab");
    }

    private static GameObject CreateLaneBlockPrefab()
    {
        GameObject root = CreateBaseBlock("Block_LaneWalls", BlockKind.LaneBlock);
        AddWallHazard(root.transform, Lane.Left, 15f, "Wall Left");
        AddWallHazard(root.transform, Lane.Center, 15f, "Wall Center");
        AddHuman(root.transform, Lane.Right, 12f);
        AddPowerUpSpawn(root.transform, Lane.Right, 21f, true, false);
        AddSideDecor(root.transform);
        return SavePrefab(root, PrefabPath + "/Block_LaneWalls.prefab");
    }

    private static GameObject CreateCartBlockPrefab()
    {
        GameObject root = CreateBaseBlock("Block_DynamicCart", BlockKind.DynamicCart);
        GameObject cartRoot = new GameObject("Dynamic Cart Root");
        cartRoot.transform.SetParent(root.transform, false);
        cartRoot.transform.localPosition = new Vector3(-3f, 0f, 17f);

        GameObject cart = CreatePrimitive(PrimitiveType.Cube, "Cart Hazard", cartRoot.transform, new Vector3(0f, 0.8f, 0f), new Vector3(2.1f, 1.4f, 2.2f), Materials["Prop"]);
        Collider cartCollider = cart.GetComponent<Collider>();
        cartCollider.isTrigger = true;
        RunnerInteractable interactable = cart.AddComponent<RunnerInteractable>();
        interactable.type = RunnerInteractableType.Hazard;
        interactable.jumpClearHeight = 99f;

        GameObject stake = CreatePrimitive(PrimitiveType.Cylinder, "Stake", cartRoot.transform, new Vector3(0f, 1.35f, -1.4f), new Vector3(0.25f, 1.8f, 0.25f), Materials["Hazard"]);
        stake.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(stake.GetComponent<Collider>());

        DynamicCartObstacle dynamicCart = root.AddComponent<DynamicCartObstacle>();
        dynamicCart.movingRoot = cartRoot.transform;
        dynamicCart.telegraphRenderer = cart.GetComponent<Renderer>();

        AddHuman(root.transform, Lane.Left, 13f);
        AddHuman(root.transform, Lane.Left, 20f);
        AddSideDecor(root.transform);
        return SavePrefab(root, PrefabPath + "/Block_DynamicCart.prefab");
    }

    private static GameObject CreatePillarBlockPrefab()
    {
        GameObject root = CreateBaseBlock("Block_FallingPillar", BlockKind.FallingPillar);
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
        return SavePrefab(root, PrefabPath + "/Block_FallingPillar.prefab");
    }

    private static GameObject CreateCatapultBlockPrefab()
    {
        GameObject projectilePrefab = CreateCatapultProjectilePrefab();
        GameObject root = CreateBaseBlock("Block_Catapult", BlockKind.Catapult);

        GameObject warning = CreatePrimitive(PrimitiveType.Cube, "Warning Strip", root.transform, new Vector3(0f, 0.04f, 15f), new Vector3(2.4f, 0.04f, 18f), Materials["Warning"]);
        Object.DestroyImmediate(warning.GetComponent<Collider>());
        warning.GetComponent<Renderer>().enabled = false;

        GameObject catapultRoot = new GameObject("Catapult");
        catapultRoot.transform.SetParent(root.transform, false);
        catapultRoot.transform.localPosition = new Vector3(0f, 0f, 27f);
        CreatePrimitive(PrimitiveType.Cube, "Catapult Base", catapultRoot.transform, new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 1.8f), Materials["Prop"]);
        GameObject arm = CreatePrimitive(PrimitiveType.Cube, "Arm", catapultRoot.transform, new Vector3(0f, 1.35f, -0.4f), new Vector3(0.35f, 0.35f, 3.2f), Materials["Wall"]);
        Object.DestroyImmediate(arm.GetComponent<Collider>());

        GameObject staticHazard = CreatePrimitive(PrimitiveType.Cube, "Catapult Body Hazard", root.transform, new Vector3(0f, 1f, 27f), new Vector3(2.7f, 2f, 2.3f), Materials["Wall"]);
        Collider hazardCollider = staticHazard.GetComponent<Collider>();
        hazardCollider.isTrigger = true;
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
        return SavePrefab(root, PrefabPath + "/Block_Catapult.prefab");
    }

    private static GameObject CreateCatapultProjectilePrefab()
    {
        GameObject root = new GameObject("Catapult_Projectile");
        GameObject visual = CreatePrimitive(PrimitiveType.Sphere, "Visual", root.transform, Vector3.zero, new Vector3(1f, 1f, 1f), Materials["Hazard"]);
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        SphereCollider collider = root.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.55f;
        RunnerInteractable interactable = root.AddComponent<RunnerInteractable>();
        interactable.type = RunnerInteractableType.Hazard;
        interactable.jumpClearHeight = 99f;
        return SavePrefab(root, PrefabPath + "/Catapult_Projectile.prefab");
    }

    private static GameObject CreateFallingBlockPrefab()
    {
        GameObject root = CreateBaseBlock("Block_FallingBlock", BlockKind.FallingBlock);
        GameObject marker = CreatePrimitive(PrimitiveType.Cylinder, "Divine Landing Marker", root.transform, new Vector3(0f, 0.05f, 15f), new Vector3(4.5f, 0.08f, 4.5f), Materials["Divine"]);
        Collider markerCollider = marker.GetComponent<Collider>();
        markerCollider.isTrigger = true;
        RunnerInteractable interactable = marker.AddComponent<RunnerInteractable>();
        interactable.type = RunnerInteractableType.RampLanding;
        interactable.jumpClearHeight = 99f;
        AddSideDecor(root.transform);
        return SavePrefab(root, PrefabPath + "/Block_FallingBlock.prefab");
    }

    private static GameObject CreateClashBlockPrefab()
    {
        GameObject root = CreateBaseBlock("Block_Clash", BlockKind.Clash);
        ClashController clash = root.AddComponent<ClashController>();

        GameObject wallRoot = new GameObject("Clash Wall Root");
        wallRoot.transform.SetParent(root.transform, false);
        wallRoot.transform.localPosition = new Vector3(0f, 0f, 18f);
        CreatePrimitive(PrimitiveType.Cube, "Main Wall", wallRoot.transform, new Vector3(0f, 2.2f, 0f), new Vector3(10.5f, 4.4f, 1.2f), Materials["Wall"]);
        CreatePrimitive(PrimitiveType.Cube, "Wall Cap", wallRoot.transform, new Vector3(0f, 4.65f, 0f), new Vector3(11f, 0.5f, 1.4f), Materials["Hazard"]);

        GameObject trigger = CreatePrimitive(PrimitiveType.Cube, "Clash Trigger", root.transform, new Vector3(0f, 1.4f, 5f), new Vector3(10f, 2.8f, 1f), Materials["Warning"]);
        trigger.GetComponent<Renderer>().enabled = false;
        Collider triggerCollider = trigger.GetComponent<Collider>();
        triggerCollider.isTrigger = true;
        RunnerInteractable triggerInteractable = trigger.AddComponent<RunnerInteractable>();
        triggerInteractable.type = RunnerInteractableType.ClashTrigger;
        triggerInteractable.jumpClearHeight = 99f;

        clash.wallRoot = wallRoot.transform;
        AddSideDecor(root.transform);
        return SavePrefab(root, PrefabPath + "/Block_Clash.prefab");
    }

    private static GameObject CreateBaseBlock(string objectName, BlockKind kind)
    {
        GameObject root = new GameObject(objectName);
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
        Collider collider = human.GetComponent<Collider>();
        collider.isTrigger = true;
        RunnerInteractable interactable = human.AddComponent<RunnerInteractable>();
        interactable.type = RunnerInteractableType.Person;
        interactable.jumpClearHeight = 99f;
        human.AddComponent<HumanPickup>();
    }

    private static void AddDestructibleBox(Transform root, Lane lane, float z, string objectName)
    {
        GameObject prop = CreatePrimitive(PrimitiveType.Cube, objectName, root, new Vector3((int)lane * 3f, 0.65f, z), new Vector3(1.4f, 1.3f, 1.4f), Materials["Prop"]);
        Collider collider = prop.GetComponent<Collider>();
        collider.isTrigger = true;
        RunnerInteractable interactable = prop.AddComponent<RunnerInteractable>();
        interactable.type = RunnerInteractableType.Destructible;
        interactable.jumpClearHeight = 99f;
        prop.AddComponent<DestructibleProp>();
    }

    private static void AddWallHazard(Transform root, Lane lane, float z, string objectName)
    {
        GameObject wall = CreatePrimitive(PrimitiveType.Cube, objectName, root, new Vector3((int)lane * 3f, 1.45f, z), new Vector3(2.6f, 2.9f, 2.2f), Materials["Wall"]);
        Collider collider = wall.GetComponent<Collider>();
        collider.isTrigger = true;
        RunnerInteractable interactable = wall.AddComponent<RunnerInteractable>();
        interactable.type = RunnerInteractableType.Hazard;
        interactable.jumpClearHeight = 99f;
    }

    private static GameObject CreatePrimitive(PrimitiveType type, string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject instance = GameObject.CreatePrimitive(type);
        instance.name = objectName;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = localScale;

        Renderer renderer = instance.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        return instance;
    }

    private static GameObject SavePrefab(GameObject root, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }

        return asset;
    }

    private static void CreateScene(RunnerConfig runnerConfig, DifficultyStageConfig[] stages, BlockDefinition[] blocks, PowerUpDefinition[] powerUps)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 300f;
        camera.fieldOfView = 62f;

        GameObject player = CreatePlayer(runnerConfig);

        GameObject coordinatorObject = new GameObject("Game Coordinator");
        GameCoordinator coordinator = coordinatorObject.AddComponent<GameCoordinator>();
        RunnerScore score = coordinatorObject.AddComponent<RunnerScore>();
        RunnerInputReader input = coordinatorObject.AddComponent<RunnerInputReader>();
        RunnerUI ui = coordinatorObject.AddComponent<RunnerUI>();

        GameObject generatorObject = new GameObject("World Generator");
        WorldGenerator generator = generatorObject.AddComponent<WorldGenerator>();
        GameObject worldRoot = new GameObject("Generated World");
        worldRoot.transform.SetParent(generatorObject.transform, false);
        generator.worldRoot = worldRoot.transform;
        generator.config = runnerConfig;
        generator.difficultyStages = stages;
        generator.blockDefinitions = blocks;
        generator.powerUps = powerUps;

        RunnerCameraController cameraController = cameraObject.AddComponent<RunnerCameraController>();
        cameraController.targetCamera = camera;
        cameraController.player = player.transform;

        PlayerRunnerController playerController = player.GetComponent<PlayerRunnerController>();
        playerController.config = runnerConfig;

        coordinator.config = runnerConfig;
        coordinator.player = playerController;
        coordinator.worldGenerator = generator;
        coordinator.score = score;
        coordinator.inputReader = input;
        coordinator.cameraController = cameraController;
        coordinator.ui = ui;

        score.config = runnerConfig;
        input.config = runnerConfig;
        input.coordinator = coordinator;
        ui.coordinator = coordinator;

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
    }

    private static GameObject CreatePlayer(RunnerConfig runnerConfig)
    {
        GameObject player = new GameObject("Player Rock");
        player.transform.position = new Vector3(0f, runnerConfig.groundHeight, 0f);
        SphereCollider collider = player.AddComponent<SphereCollider>();
        collider.radius = 0.75f;
        Rigidbody body = player.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        GameObject visual = CreatePrimitive(PrimitiveType.Sphere, "Rock Visual", player.transform, Vector3.zero, new Vector3(1.5f, 1.5f, 1.5f), Materials["Rock"]);
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        PlayerRunnerController controller = player.AddComponent<PlayerRunnerController>();
        controller.visualRoot = visual.transform;
        controller.config = runnerConfig;
        return player;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool found = false;
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == scenePath)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                found = true;
                break;
            }
        }

        if (!found)
        {
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
