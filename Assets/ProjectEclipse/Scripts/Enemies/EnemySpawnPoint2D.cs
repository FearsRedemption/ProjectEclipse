using UnityEngine;

namespace ProjectEclipse.Enemies
{
    public class EnemySpawnPoint2D : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private int maxAlive = -1;
        [SerializeField] private float spawnRadius = 3.8f;
        [SerializeField] private float respawnSeconds = 7.5f;
        [SerializeField] private float respawnJitterSeconds = 2.5f;
        [SerializeField] private float emptyRespawnMultiplier = 0.28f;
        [SerializeField] private bool spawnOnStartIfEmpty = true;

        public EnemyDefinition Definition { get { return definition; } }
        public int MaxAlive { get { return maxAlive; } }
        public float SpawnRadius { get { return Mathf.Max(0f, spawnRadius); } }
        public float RespawnSeconds { get { return Mathf.Max(0.1f, respawnSeconds); } }
        public float RespawnJitterSeconds { get { return Mathf.Max(0f, respawnJitterSeconds); } }
        public float EmptyRespawnMultiplier { get { return Mathf.Clamp(emptyRespawnMultiplier, 0.12f, 1f); } }
        public bool SpawnOnStartIfEmpty { get { return spawnOnStartIfEmpty; } }

        public void Configure(EnemyDefinition enemyDefinition, int aliveCap, float radius, float respawn, float jitter)
        {
            definition = enemyDefinition;
            maxAlive = aliveCap;
            spawnRadius = Mathf.Max(0f, radius);
            respawnSeconds = Mathf.Max(0.1f, respawn);
            respawnJitterSeconds = Mathf.Max(0f, jitter);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.95f, 0.35f, 0.18f, 0.35f);
            Gizmos.DrawWireCube(transform.position, new Vector3(SpawnRadius * 2f, 1.2f, 0.1f));
        }
    }
}
