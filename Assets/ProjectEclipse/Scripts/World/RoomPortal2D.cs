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
        [SerializeField] private float reuseDelay = 0.45f;

        private float nextUseTime;
        private PlayerController nearbyPlayer;

        public void Configure(RoomBounds2D room, Transform spawn)
        {
            targetRoom = room;
            targetSpawn = spawn;
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
            if (Time.time < nextUseTime || targetSpawn == null || nearbyPlayer == null || !WantsPortalUse())
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
            nextUseTime = Time.time + Mathf.Max(0.05f, reuseDelay);
            nearbyPlayer = null;
            player.transform.position = targetSpawn.position;

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
                if (targetRoom != null)
                {
                    follow.SetBounds(targetRoom.Bounds);
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
    }
}
