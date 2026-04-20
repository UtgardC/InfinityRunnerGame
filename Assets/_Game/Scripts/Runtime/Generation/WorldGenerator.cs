using System.Collections.Generic;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class WorldGenerator : MonoBehaviour
    {
        public RunnerConfig config;
        public DifficultyStageConfig[] difficultyStages;
        public BlockDefinition[] blockDefinitions;
        public PowerUpDefinition[] powerUps;
        public Transform worldRoot;

        private readonly List<BlockRuntime> activeBlocks = new List<BlockRuntime>();
        private readonly Dictionary<BlockDefinition, Queue<GameObject>> blockPools = new Dictionary<BlockDefinition, Queue<GameObject>>();
        private readonly Dictionary<GameObject, Queue<GameObject>> prefabPools = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly List<BlockDefinition> weightedCandidates = new List<BlockDefinition>();

        private int stageIndex;
        private float nextSpawnZ;
        private int blocksSinceClash;
        private int lateBlocksSinceClash;
        private int blocksUntilPowerUp;
        private PowerUpDefinition pendingPowerUp;
        private bool forceFallingBlock;
        private bool running;

        public DifficultyStage CurrentStage
        {
            get
            {
                DifficultyStageConfig stage = CurrentStageConfig;
                return stage != null ? stage.stage : DifficultyStage.Start;
            }
        }

        public float CurrentSpeed
        {
            get
            {
                DifficultyStageConfig stage = CurrentStageConfig;
                return stage != null ? stage.speedMetersPerSecond : 0f;
            }
        }

        public DifficultyStageConfig CurrentStageConfig
        {
            get
            {
                if (difficultyStages == null || difficultyStages.Length == 0)
                {
                    return null;
                }

                stageIndex = Mathf.Clamp(stageIndex, 0, difficultyStages.Length - 1);
                return difficultyStages[stageIndex];
            }
        }

        private void Awake()
        {
            if (worldRoot == null)
            {
                GameObject root = new GameObject("Generated World");
                root.transform.SetParent(transform, false);
                worldRoot = root.transform;
            }
        }

        private void Update()
        {
            if (!running || GameCoordinator.Instance == null || GameCoordinator.Instance.State != RunnerState.Running)
            {
                return;
            }

            float distance = CurrentSpeed * Time.deltaTime;
            MoveWorld(distance);
            GameCoordinator.Instance.AddDistance(distance);
            EnsureBlocksAhead();
            DespawnPassedBlocks();
        }

        public void ResetGenerator()
        {
            running = false;
            stageIndex = 0;
            nextSpawnZ = 0f;
            blocksSinceClash = 0;
            lateBlocksSinceClash = 999;
            blocksUntilPowerUp = Mathf.Max(1, config != null ? config.powerUpCooldownBlocks : 3);
            pendingPowerUp = null;
            forceFallingBlock = false;

            for (int i = activeBlocks.Count - 1; i >= 0; i--)
            {
                DespawnBlock(activeBlocks[i]);
            }

            activeBlocks.Clear();
        }

        public void BeginGeneration()
        {
            ResetGenerator();
            running = true;
            EnsureBlocksAhead();
        }

        public void StopGeneration()
        {
            running = false;
        }

        public void ResumeGeneration()
        {
            running = true;
        }

        public void ScheduleFallingBlock()
        {
            forceFallingBlock = true;
        }

        public void AdvanceDifficultyAfterClash()
        {
            blocksSinceClash = 0;
            lateBlocksSinceClash = 0;

            if (stageIndex < difficultyStages.Length - 1)
            {
                stageIndex++;
            }
        }

        private void MoveWorld(float distance)
        {
            for (int i = 0; i < activeBlocks.Count; i++)
            {
                if (activeBlocks[i].Root != null)
                {
                    activeBlocks[i].Root.transform.position += Vector3.back * distance;
                }
            }

            nextSpawnZ -= distance;
        }

        private void EnsureBlocksAhead()
        {
            if (config == null)
            {
                return;
            }

            while (nextSpawnZ < config.spawnAheadDistance && activeBlocks.Count < config.maxBlocksActive)
            {
                BlockDefinition definition = SelectNextBlock();
                if (definition == null)
                {
                    return;
                }

                SpawnBlock(definition, nextSpawnZ);
                nextSpawnZ += Mathf.Max(5f, definition.length);
            }
        }

        private BlockDefinition SelectNextBlock()
        {
            if (forceFallingBlock)
            {
                BlockDefinition falling = FindSpecialBlock(BlockKind.FallingBlock);
                if (falling != null)
                {
                    forceFallingBlock = false;
                    return falling;
                }
            }

            BlockDefinition clash = FindSpecialBlock(BlockKind.Clash);
            DifficultyStageConfig stageConfig = CurrentStageConfig;
            if (stageConfig != null && clash != null)
            {
                bool shouldForceProgressionClash = stageIndex < difficultyStages.Length - 1 && blocksSinceClash >= stageConfig.blocksBeforeForcedClash;
                bool shouldLateClash = stageIndex == difficultyStages.Length - 1
                    && lateBlocksSinceClash >= stageConfig.minimumBlocksBetweenLateClashes
                    && Random.value <= stageConfig.lateClashChance;

                if (shouldForceProgressionClash || shouldLateClash)
                {
                    blocksSinceClash = 0;
                    lateBlocksSinceClash = 0;
                    return clash;
                }
            }

            weightedCandidates.Clear();
            for (int i = 0; i < blockDefinitions.Length; i++)
            {
                BlockDefinition definition = blockDefinitions[i];
                if (definition == null || definition.prefab == null || definition.weight <= 0 || definition.kind == BlockKind.Clash || definition.kind == BlockKind.FallingBlock)
                {
                    continue;
                }

                if (!definition.IsAllowed(CurrentStage))
                {
                    continue;
                }

                for (int w = 0; w < definition.weight; w++)
                {
                    weightedCandidates.Add(definition);
                }
            }

            if (weightedCandidates.Count == 0)
            {
                return FindFallbackSafeBlock();
            }

            return weightedCandidates[Random.Range(0, weightedCandidates.Count)];
        }

        private void SpawnBlock(BlockDefinition definition, float z)
        {
            GameObject instance = GetBlockInstance(definition);
            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(worldRoot, false);
            instanceTransform.position = new Vector3(0f, 0f, z);
            instanceTransform.rotation = Quaternion.identity;
            instance.SetActive(true);

            BlockMetadata metadata = instance.GetComponent<BlockMetadata>();
            if (metadata != null)
            {
                metadata.length = definition.length;
                metadata.kind = definition.kind;
                ResetInteractables(instance);
                SpawnPowerUpIfNeeded(definition, metadata);
            }

            activeBlocks.Add(new BlockRuntime(definition, instance, metadata));

            if (definition.kind != BlockKind.Clash && definition.kind != BlockKind.FallingBlock)
            {
                blocksSinceClash++;
                lateBlocksSinceClash++;
            }
        }

        private GameObject GetBlockInstance(BlockDefinition definition)
        {
            Queue<GameObject> pool;
            if (blockPools.TryGetValue(definition, out pool) && pool.Count > 0)
            {
                return pool.Dequeue();
            }

            return Instantiate(definition.prefab);
        }

        private void DespawnPassedBlocks()
        {
            if (config == null)
            {
                return;
            }

            for (int i = activeBlocks.Count - 1; i >= 0; i--)
            {
                BlockRuntime block = activeBlocks[i];
                float length = block.Definition != null ? block.Definition.length : 30f;
                if (block.Root.transform.position.z + length < -config.despawnBehindDistance)
                {
                    activeBlocks.RemoveAt(i);
                    DespawnBlock(block);
                }
            }
        }

        private void DespawnBlock(BlockRuntime block)
        {
            if (block.Root == null)
            {
                return;
            }

            if (block.Metadata != null)
            {
                IReadOnlyList<GameObject> spawnedObjects = block.Metadata.SpawnedPooledObjects;
                for (int i = 0; i < spawnedObjects.Count; i++)
                {
                    ReturnPooledPrefab(spawnedObjects[i]);
                }
                block.Metadata.ClearSpawnedObjects();
            }

            block.Root.SetActive(false);
            Queue<GameObject> pool;
            if (!blockPools.TryGetValue(block.Definition, out pool))
            {
                pool = new Queue<GameObject>();
                blockPools.Add(block.Definition, pool);
            }
            pool.Enqueue(block.Root);
        }

        private void SpawnPowerUpIfNeeded(BlockDefinition definition, BlockMetadata metadata)
        {
            if (config == null || powerUps == null || powerUps.Length == 0 || definition.kind == BlockKind.Clash || definition.kind == BlockKind.FallingBlock)
            {
                return;
            }

            if (pendingPowerUp == null)
            {
                if (blocksUntilPowerUp > 0)
                {
                    blocksUntilPowerUp--;
                }
                else if (Random.value <= config.powerUpChancePerEligibleBlock)
                {
                    pendingPowerUp = powerUps[Random.Range(0, powerUps.Length)];
                }
            }

            if (pendingPowerUp == null)
            {
                return;
            }

            PowerUpSpawnPoint[] points = metadata.GetPowerUpSpawnPoints();
            List<PowerUpSpawnPoint> compatible = new List<PowerUpSpawnPoint>();
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null && points[i].Allows(pendingPowerUp.type))
                {
                    compatible.Add(points[i]);
                }
            }

            if (compatible.Count == 0)
            {
                return;
            }

            PowerUpSpawnPoint point = compatible[Random.Range(0, compatible.Count)];
            GameObject pickup = SpawnPooledPrefab(pendingPowerUp.pickupPrefab, point.transform.position, point.transform.rotation, metadata.transform);
            PowerUpPickup powerUpPickup = pickup.GetComponent<PowerUpPickup>();
            if (powerUpPickup != null)
            {
                powerUpPickup.definition = pendingPowerUp;
            }

            metadata.RegisterSpawnedObject(pickup);

            if (pendingPowerUp.requiresFallingBlockAfterPickup || pendingPowerUp.type == PowerUpType.DivineRamp)
            {
                ScheduleFallingBlock();
            }

            pendingPowerUp = null;
            blocksUntilPowerUp = config.powerUpCooldownBlocks;
        }

        private GameObject SpawnPooledPrefab(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (prefab == null)
            {
                return null;
            }

            Queue<GameObject> pool;
            GameObject instance;
            if (prefabPools.TryGetValue(prefab, out pool) && pool.Count > 0)
            {
                instance = pool.Dequeue();
            }
            else
            {
                instance = Instantiate(prefab);
                PooledPrefabMarker marker = instance.GetComponent<PooledPrefabMarker>();
                if (marker == null)
                {
                    marker = instance.AddComponent<PooledPrefabMarker>();
                }
                marker.prefab = prefab;
            }

            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);
            ResetInteractables(instance);
            return instance;
        }

        private void ReturnPooledPrefab(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            PooledPrefabMarker marker = instance.GetComponent<PooledPrefabMarker>();
            if (marker == null || marker.prefab == null)
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);

            Queue<GameObject> pool;
            if (!prefabPools.TryGetValue(marker.prefab, out pool))
            {
                pool = new Queue<GameObject>();
                prefabPools.Add(marker.prefab, pool);
            }

            pool.Enqueue(instance);
        }

        private void ResetInteractables(GameObject root)
        {
            RunnerInteractable[] interactables = root.GetComponentsInChildren<RunnerInteractable>(true);
            for (int i = 0; i < interactables.Length; i++)
            {
                interactables[i].ResetInteraction();
            }
        }

        private BlockDefinition FindSpecialBlock(BlockKind kind)
        {
            if (blockDefinitions == null)
            {
                return null;
            }

            for (int i = 0; i < blockDefinitions.Length; i++)
            {
                BlockDefinition definition = blockDefinitions[i];
                if (definition != null && definition.kind == kind && definition.prefab != null)
                {
                    return definition;
                }
            }

            return null;
        }

        private BlockDefinition FindFallbackSafeBlock()
        {
            if (blockDefinitions == null)
            {
                return null;
            }

            for (int i = 0; i < blockDefinitions.Length; i++)
            {
                BlockDefinition definition = blockDefinitions[i];
                if (definition != null && definition.kind == BlockKind.Safe && definition.prefab != null)
                {
                    return definition;
                }
            }

            return blockDefinitions.Length > 0 ? blockDefinitions[0] : null;
        }

        private readonly struct BlockRuntime
        {
            public readonly BlockDefinition Definition;
            public readonly GameObject Root;
            public readonly BlockMetadata Metadata;

            public BlockRuntime(BlockDefinition definition, GameObject root, BlockMetadata metadata)
            {
                Definition = definition;
                Root = root;
                Metadata = metadata;
            }
        }
    }
}
