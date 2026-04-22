using System.Collections.Generic;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class WorldGenerator : MonoBehaviour
    {
        public RunnerConfig config;
        public DifficultyStageConfig[] difficultyStages;
        public BlockDefinition initialBlockDefinition;
        public BlockDefinition[] blockDefinitions;
        public Transform worldRoot;

        private readonly List<BlockRuntime> activeBlocks = new List<BlockRuntime>();
        private readonly Dictionary<BlockDefinition, Queue<GameObject>> blockPools = new Dictionary<BlockDefinition, Queue<GameObject>>();
        private readonly List<BlockDefinition> weightedCandidates = new List<BlockDefinition>();

        private int stageIndex;
        private int spawnedBlockCount;
        private float nextSpawnLocalZ;
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
                worldRoot = transform;
            }
        }

        private void Update()
        {
            if (!running || config == null || worldRoot == null)
            {
                return;
            }

            float distance = CurrentSpeed * Time.deltaTime;
            MoveBlocks(distance);
            if (GameCoordinator.Instance != null)
            {
                GameCoordinator.Instance.AddDistance(distance);
            }
            EnsureBlocksAhead();
            DespawnPassedBlocks();
        }

        public void BeginRun()
        {
            ResetWorld();
            SpawnInitialBlockIfAssigned();
            running = true;
            EnsureBlocksAhead();
        }

        public void StopRun()
        {
            running = false;
        }

        public void ResetWorld()
        {
            running = false;
            stageIndex = 0;
            spawnedBlockCount = 0;
            nextSpawnLocalZ = 0f;

            if (worldRoot != null)
            {
                worldRoot.localPosition = Vector3.zero;
            }

            for (int i = activeBlocks.Count - 1; i >= 0; i--)
            {
                DespawnBlock(activeBlocks[i]);
            }

            activeBlocks.Clear();
        }

        private void MoveBlocks(float distance)
        {
            for (int i = 0; i < activeBlocks.Count; i++)
            {
                GameObject root = activeBlocks[i].Root;
                if (root == null)
                {
                    continue;
                }

                root.transform.localPosition += Vector3.back * distance;
            }

            nextSpawnLocalZ -= distance;
        }

        private void EnsureBlocksAhead()
        {
            if (config == null || worldRoot == null)
            {
                return;
            }

            while (ShouldSpawnAnotherBlock())
            {
                UpdateStageFromProgress();
                BlockDefinition definition = SelectNextBlock();
                if (definition == null)
                {
                    return;
                }

                SpawnBlock(definition, nextSpawnLocalZ);
            }
        }

        private void SpawnInitialBlockIfAssigned()
        {
            if (initialBlockDefinition == null || initialBlockDefinition.prefab == null)
            {
                return;
            }

            SpawnBlock(initialBlockDefinition, 0f, false, false);
        }

        private bool ShouldSpawnAnotherBlock()
        {
            if (activeBlocks.Count >= config.maxBlocksActive)
            {
                return false;
            }

            if (activeBlocks.Count == 0)
            {
                return true;
            }

            BlockRuntime lastBlock = activeBlocks[activeBlocks.Count - 1];
            return lastBlock.Root.transform.localPosition.z + lastBlock.Length < config.spawnAheadDistance;
        }

        private BlockDefinition SelectNextBlock()
        {
            weightedCandidates.Clear();

            if (blockDefinitions == null)
            {
                return null;
            }

            for (int i = 0; i < blockDefinitions.Length; i++)
            {
                BlockDefinition definition = blockDefinitions[i];
                if (definition == null || definition.prefab == null || definition.weight <= 0)
                {
                    continue;
                }

                if (!definition.Allows(CurrentStage))
                {
                    continue;
                }

                for (int weightIndex = 0; weightIndex < definition.weight; weightIndex++)
                {
                    weightedCandidates.Add(definition);
                }
            }

            if (weightedCandidates.Count == 0)
            {
                return null;
            }

            return weightedCandidates[Random.Range(0, weightedCandidates.Count)];
        }

        private void SpawnBlock(BlockDefinition definition, float localZ)
        {
            SpawnBlock(definition, localZ, true, true);
        }

        private void SpawnBlock(BlockDefinition definition, float localZ, bool returnToPool, bool countsAsProgress)
        {
            GameObject instance = GetBlockInstance(definition, returnToPool);
            BlockMetadata metadata = instance.GetComponent<BlockMetadata>();
            if (metadata == null)
            {
                Debug.LogError("Each block prefab needs a BlockMetadata component.", instance);
                instance.SetActive(false);
                return;
            }

            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(worldRoot, false);
            instanceTransform.localPosition = new Vector3(0f, 0f, localZ);
            instanceTransform.rotation = Quaternion.identity;
            instance.SetActive(true);
            ResetInteractables(instance);

            activeBlocks.Add(new BlockRuntime(definition, instance, metadata.length, returnToPool));
            nextSpawnLocalZ += metadata.length;
            if (countsAsProgress)
            {
                spawnedBlockCount++;
            }
        }

        private GameObject GetBlockInstance(BlockDefinition definition, bool canUsePool)
        {
            if (!canUsePool)
            {
                return Instantiate(definition.prefab);
            }

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
                if (block.Root.transform.localPosition.z + block.Length < -config.despawnBehindDistance)
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

            block.Root.SetActive(false);
            if (!block.ReturnToPool)
            {
                Destroy(block.Root);
                return;
            }

            block.Root.transform.SetParent(transform, false);

            Queue<GameObject> pool;
            if (!blockPools.TryGetValue(block.Definition, out pool))
            {
                pool = new Queue<GameObject>();
                blockPools.Add(block.Definition, pool);
            }

            pool.Enqueue(block.Root);
        }

        private void ResetInteractables(GameObject root)
        {
            RunnerInteractable[] interactables = root.GetComponentsInChildren<RunnerInteractable>(true);
            for (int i = 0; i < interactables.Length; i++)
            {
                interactables[i].ResetInteraction();
            }
        }

        private void UpdateStageFromProgress()
        {
            if (difficultyStages == null || difficultyStages.Length == 0)
            {
                stageIndex = 0;
                return;
            }

            int resolvedStageIndex = 0;
            for (int i = 0; i < difficultyStages.Length; i++)
            {
                DifficultyStageConfig stage = difficultyStages[i];
                if (stage != null && spawnedBlockCount >= stage.startAfterBlockCount)
                {
                    resolvedStageIndex = i;
                }
            }

            stageIndex = Mathf.Clamp(resolvedStageIndex, 0, difficultyStages.Length - 1);
        }

        private readonly struct BlockRuntime
        {
            public readonly BlockDefinition Definition;
            public readonly GameObject Root;
            public readonly float Length;
            public readonly bool ReturnToPool;

            public BlockRuntime(BlockDefinition definition, GameObject root, float length, bool returnToPool)
            {
                Definition = definition;
                Root = root;
                Length = length;
                ReturnToPool = returnToPool;
            }
        }
    }
}
