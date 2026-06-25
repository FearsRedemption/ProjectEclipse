using System.Collections.Generic;
using ProjectEclipse.Player;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.World
{
    [RequireComponent(typeof(Collider2D))]
    public class RoomPortal2D : MonoBehaviour
    {
        [SerializeField] private RoomBounds2D targetRoom;
        [SerializeField] private Transform targetSpawn;
        [SerializeField] private RoomBounds2D owningRoom;
        [SerializeField] private Transform arrivalPoint;
        [SerializeField] private RoomPortal2D linkedPortal;
        [SerializeField] private float reuseDelay = 0.45f;
        [SerializeField] private float playerCooldownSeconds = 2.25f;

        private static readonly Dictionary<int, float> PlayerCooldowns = new Dictionary<int, float>();
        private float nextUseTime;
        private PlayerController nearbyPlayer;

        public RoomBounds2D OwningRoom { get { return owningRoom; } }
        public Transform ArrivalPoint { get { return arrivalPoint != null ? arrivalPoint : transform; } }
        public RoomPortal2D LinkedPortal { get { return linkedPortal; } }

        public void Configure(RoomBounds2D room, Transform spawn)
        {
            targetRoom = room;
            targetSpawn = spawn;
        }

        public void Configure(RoomBounds2D ownerRoom, Transform arrival, RoomPortal2D destination)
        {
            owningRoom = ownerRoom;
            arrivalPoint = arrival;
            LinkTo(destination);
        }

        public void LinkTo(RoomPortal2D destination)
        {
            linkedPortal = destination;
            if (linkedPortal != null)
            {
                targetRoom = linkedPortal.OwningRoom;
                targetSpawn = linkedPortal.ArrivalPoint;
            }
        }

        public void SuppressUseFor(float seconds)
        {
            nextUseTime = Mathf.Max(nextUseTime, Time.time + Mathf.Max(0.05f, seconds));
            nearbyPlayer = null;
        }

        private void Reset()
        {
            Collider2D portalCollider = GetComponent<Collider2D>();
            if (portalCollider != null)
            {
                portalCollider.isTrigger = true;
            }
        }

        private void Awake()
        {
            Collider2D portalCollider = GetComponent<Collider2D>();
            if (portalCollider != null)
            {
                portalCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            if (Time.time < nextUseTime || !HasDestination() || nearbyPlayer == null || IsPlayerOnPortalCooldown(nearbyPlayer) || !WantsPortalUse())
            {
                return;
            }

            Transfer(nearbyPlayer);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other != null ? other.GetComponentInParent<PlayerController>() : null;
            if (player != null)
            {
                nearbyPlayer = player;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (nearbyPlayer != null)
            {
                return;
            }

            OnTriggerEnter2D(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            PlayerController player = other != null ? other.GetComponentInParent<PlayerController>() : null;
            if (player != null && player == nearbyPlayer)
            {
                nearbyPlayer = null;
            }
        }

        private void Transfer(PlayerController player)
        {
            RoomPortal2D destination = linkedPortal;
            Transform destinationSpawn = destination != null ? destination.ArrivalPoint : targetSpawn;
            RoomBounds2D destinationRoom = destination != null ? destination.OwningRoom : targetRoom;
            if (destinationSpawn == null)
            {
                return;
            }

            nextUseTime = Time.time + Mathf.Max(0.05f, reuseDelay);
            SetPlayerPortalCooldown(player);
            if (destination != null)
            {
                destination.SuppressUseFor(reuseDelay);
            }

            nearbyPlayer = null;
            player.transform.position = destinationSpawn.position;

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }

            Camera camera = Camera.main;
            CameraFollow2D follow = camera != null ? camera.GetComponent<CameraFollow2D>() : null;
            if (follow != null)
            {
                follow.SetTarget(player.transform);
                if (destinationRoom != null)
                {
                    follow.SetBounds(destinationRoom.Bounds);
                }
                follow.SnapToTarget();
            }
        }

        private static bool WantsPortalUse()
        {
            return Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.UpArrow)
                || Input.GetKeyDown(KeyCode.Return);
        }

        private bool HasDestination()
        {
            return linkedPortal != null || targetSpawn != null;
        }

        private bool IsPlayerOnPortalCooldown(PlayerController player)
        {
            if (player == null)
            {
                return false;
            }

            float cooldownUntil;
            int key = player.GetInstanceID();
            if (!PlayerCooldowns.TryGetValue(key, out cooldownUntil))
            {
                return false;
            }

            if (Time.time < cooldownUntil)
            {
                return true;
            }

            PlayerCooldowns.Remove(key);
            return false;
        }

        private void SetPlayerPortalCooldown(PlayerController player)
        {
            if (player == null)
            {
                return;
            }

            PlayerCooldowns[player.GetInstanceID()] = Time.time + Mathf.Max(0.1f, playerCooldownSeconds);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.35f, 0.85f, 1f, 0.85f);
            Gizmos.DrawWireCube(transform.position, new Vector3(0.9f, 1.55f, 0.1f));
            if (linkedPortal != null)
            {
                Gizmos.DrawLine(transform.position, linkedPortal.transform.position);
            }

            Transform arrival = ArrivalPoint;
            if (arrival != null)
            {
                Gizmos.color = new Color(0.4f, 1f, 0.55f, 0.85f);
                Gizmos.DrawWireSphere(arrival.position, 0.18f);
            }
        }
    }
}
