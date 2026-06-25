using System.Collections.Generic;
using ProjectEclipse.Items;
using ProjectEclipse.Progression;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Enemies
{
    public class EnemySpawnManager : MonoBehaviour
    {
        private class SpawnGroup
        {
            public EnemyDefinition Definition;
            public Vector3 Center;
            public readonly List<EnemyController> ActiveEnemies = new List<EnemyController>();
            public float NextSpawnTime;
            public int MaxAliveOverride = -1;
            public float RespawnSecondsOverride = -1f;
            public float RespawnJitterOverride = -1f;
            public float SpawnRadiusOverride = -1f;
            public float EmptyRespawnMultiplier = 0.28f;
        }

        [SerializeField] private int earlyMaxPerPlatform = 5;
        [SerializeField] private int laterMaxPerPlatform = 50;
        [SerializeField] private float platformGroupWidth = 10f;
        [SerializeField] private float spawnRadius = 3.8f;
        [SerializeField] private float respawnSeconds = 7.5f;
        [SerializeField] private float respawnJitterSeconds = 2.5f;

        private readonly List<SpawnGroup> groups = new List<SpawnGroup>();
        private Transform playerTarget;
        private DropSpawner dropSpawner;
        private bool initialized;

        public int GroupCount { get { return groups.Count; } }

        public void Initialize(IEnumerable<EnemyController> placedEnemies, Transform target, DropSpawner spawner)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            playerTarget = target;
            dropSpawner = spawner;

            if (placedEnemies != null)
            {
                foreach (EnemyController enemy in placedEnemies)
                {
                    if (enemy == null || enemy.Definition == null)
                    {
                        continue;
                    }

                    enemy.Initialize(enemy.Definition, playerTarget, dropSpawner);
                    SpawnGroup group = GetOrCreateGroup(enemy.Definition, enemy.transform.position);
                    group.ActiveEnemies.Add(enemy);
                }
            }

            RegisterAuthoredSpawnPoints();
        }

        private void Update()
        {
            for (int i = 0; i < groups.Count; i++)
            {
                TickGroup(groups[i]);
            }
        }

        private void TickGroup(SpawnGroup group)
        {
            if (group == null || group.Definition == null)
            {
                return;
            }

            RemoveMissingEnemies(group);
            int maxAlive = GetMaxFor(group);
            if (group.ActiveEnemies.Count >= maxAlive || Time.time < group.NextSpawnTime)
            {
                return;
            }

            int aliveBeforeSpawn = group.ActiveEnemies.Count;
            EnemyController spawned = SpawnEnemy(group);
            if (spawned != null)
            {
                group.ActiveEnemies.Add(spawned);
            }

            group.NextSpawnTime = Time.time + GetRespawnDelay(group, maxAlive, aliveBeforeSpawn);
        }

        private SpawnGroup GetOrCreateGroup(EnemyDefinition definition, Vector3 position)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                SpawnGroup group = groups[i];
                if (group.Definition == definition
                    && Mathf.Abs(group.Center.y - position.y) < 1.5f
                    && Mathf.Abs(group.Center.x - position.x) <= Mathf.Max(1f, platformGroupWidth))
                {
                    return group;
                }
            }

            SpawnGroup created = new SpawnGroup
            {
                Definition = definition,
                Center = position,
                NextSpawnTime = Time.time + Random.Range(1f, Mathf.Max(1.1f, respawnSeconds))
            };
            groups.Add(created);
            return created;
        }

        private EnemyController SpawnEnemy(SpawnGroup group)
        {
            float radius = group.SpawnRadiusOverride >= 0f ? group.SpawnRadiusOverride : spawnRadius;
            Vector3 position = group.Center + new Vector3(Random.Range(-radius, radius), 0f, 0f);
            GameObject enemyObject = new GameObject(group.Definition.DisplayName);
            enemyObject.transform.position = position;

            SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 8;

            Rigidbody2D body = enemyObject.AddComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.gravityScale = 3f;

            enemyObject.AddComponent<BoxCollider2D>();
            enemyObject.AddComponent<SpriteSheetAnimator>();
            enemyObject.AddComponent<VisualStateAnimator>();
            EnemyController enemy = enemyObject.AddComponent<EnemyController>();
            enemy.Initialize(group.Definition, playerTarget, dropSpawner);
            return enemy;
        }

        private void RegisterAuthoredSpawnPoints()
        {
            EnemySpawnPoint2D[] spawnPoints = FindObjectsByType<EnemySpawnPoint2D>(FindObjectsSortMode.None);
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                EnemySpawnPoint2D point = spawnPoints[i];
                if (point == null || point.Definition == null)
                {
                    continue;
                }

                SpawnGroup group = GetOrCreateGroup(point.Definition, point.transform.position);
                group.Center = point.transform.position;
                group.MaxAliveOverride = point.MaxAlive;
                group.RespawnSecondsOverride = point.RespawnSeconds;
                group.RespawnJitterOverride = point.RespawnJitterSeconds;
                group.SpawnRadiusOverride = point.SpawnRadius;
                group.EmptyRespawnMultiplier = point.EmptyRespawnMultiplier;
                if (point.SpawnOnStartIfEmpty && group.ActiveEnemies.Count == 0)
                {
                    group.NextSpawnTime = Mathf.Min(group.NextSpawnTime, Time.time + Random.Range(0.2f, 0.85f));
                }
            }
        }

        private void RemoveMissingEnemies(SpawnGroup group)
        {
            for (int i = group.ActiveEnemies.Count - 1; i >= 0; i--)
            {
                EnemyController enemy = group.ActiveEnemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    group.ActiveEnemies.RemoveAt(i);
                }
            }
        }

        private float GetRespawnDelay(SpawnGroup group, int maxAlive, int aliveCount)
        {
            float baseSeconds = group.RespawnSecondsOverride >= 0f ? group.RespawnSecondsOverride : respawnSeconds;
            float jitter = group.RespawnJitterOverride >= 0f ? group.RespawnJitterOverride : respawnJitterSeconds;
            float fill = maxAlive > 0 ? Mathf.Clamp01(aliveCount / (float)maxAlive) : 1f;
            float multiplier = Mathf.Lerp(Mathf.Clamp(group.EmptyRespawnMultiplier, 0.12f, 1f), 1f, fill);
            return Mathf.Max(0.35f, baseSeconds * multiplier) + Random.Range(0f, Mathf.Max(0f, jitter) * multiplier);
        }

        private int GetMaxFor(SpawnGroup group)
        {
            if (group != null && group.MaxAliveOverride > 0)
            {
                return group.MaxAliveOverride;
            }

            EnemyDefinition definition = group != null ? group.Definition : null;
            if (definition == null)
            {
                return Mathf.Max(1, earlyMaxPerPlatform);
            }

            return IsEarlyTier(definition.ResourceTier)
                ? Mathf.Max(1, earlyMaxPerPlatform)
                : Mathf.Max(earlyMaxPerPlatform, laterMaxPerPlatform);
        }

        private static bool IsEarlyTier(ResourceTier tier)
        {
            return tier == ResourceTier.Wood
                || tier == ResourceTier.Stone
                || tier == ResourceTier.Coal
                || tier == ResourceTier.Copper;
        }
    }
}
