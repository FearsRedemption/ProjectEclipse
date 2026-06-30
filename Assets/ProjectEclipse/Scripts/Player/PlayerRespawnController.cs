using ProjectEclipse.Combat;
using ProjectEclipse.Equipment;
using ProjectEclipse.Utilities;
using ProjectEclipse.World;
using UnityEngine;

namespace ProjectEclipse.Player
{
    public class PlayerRespawnController : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private PlayerResource resource;
        [SerializeField] private MvpRoomFlowBuilder roomFlowBuilder;
        [SerializeField] private float respawnDelaySeconds = 10f;

        private Rigidbody2D body;
        private VisualStateAnimator visualState;
        private CharacterVisualController characterVisuals;
        private bool previousBodySimulation = true;
        private bool subscribed;
        private float respawnAt = -1f;
        private Vector3 fallbackRespawnPosition;

        public bool IsRespawning { get; private set; }
        public bool BlocksPlayerInput { get { return IsRespawning; } }
        public float RemainingSeconds { get { return IsRespawning ? Mathf.Max(0f, respawnAt - Time.time) : 0f; } }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visualState = GetComponent<VisualStateAnimator>();
            characterVisuals = GetComponent<CharacterVisualController>();
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (resource == null)
            {
                resource = GetComponent<PlayerResource>();
            }

            fallbackRespawnPosition = transform.position;
            Subscribe();
        }

        private void OnDestroy()
        {
            if (health != null && subscribed)
            {
                health.Died -= HandleDied;
            }
        }

        private void Update()
        {
            if (!IsRespawning)
            {
                return;
            }

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }

            if (Time.time >= respawnAt)
            {
                Respawn();
            }
        }

        public void Initialize(Health playerHealth, PlayerResource playerResource, MvpRoomFlowBuilder flowBuilder)
        {
            if (health != null && subscribed)
            {
                health.Died -= HandleDied;
                subscribed = false;
            }

            health = playerHealth != null ? playerHealth : health;
            resource = playerResource != null ? playerResource : resource;
            roomFlowBuilder = flowBuilder != null ? flowBuilder : roomFlowBuilder;
            Subscribe();
        }

        private void Subscribe()
        {
            if (health == null || subscribed)
            {
                return;
            }

            health.Died += HandleDied;
            subscribed = true;
        }

        private void HandleDied()
        {
            if (IsRespawning)
            {
                return;
            }

            IsRespawning = true;
            respawnAt = Time.time + Mathf.Max(0.1f, respawnDelaySeconds);
            if (body != null)
            {
                previousBodySimulation = body.simulated;
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }
        }

        private void Respawn()
        {
            Vector3 respawnPosition = roomFlowBuilder != null
                ? roomFlowBuilder.GetSafeRespawnPosition()
                : fallbackRespawnPosition;

            transform.position = respawnPosition;
            if (body != null)
            {
                body.simulated = previousBodySimulation;
                body.linearVelocity = Vector2.zero;
            }

            if (health != null)
            {
                health.ReviveToFull();
            }

            if (resource != null)
            {
                resource.RestoreToFull();
            }

            ResetVisualState();

            if (roomFlowBuilder != null)
            {
                roomFlowBuilder.ApplyCameraBoundsForPlayer(transform);
            }

            IsRespawning = false;
            respawnAt = -1f;
        }

        private void ResetVisualState()
        {
            if (visualState == null)
            {
                visualState = GetComponent<VisualStateAnimator>();
            }

            if (characterVisuals == null)
            {
                characterVisuals = GetComponent<CharacterVisualController>();
            }

            if (visualState != null)
            {
                visualState.ResetVisualState();
            }

            if (characterVisuals != null)
            {
                characterVisuals.ResetVisualState();
            }
        }
    }
}
