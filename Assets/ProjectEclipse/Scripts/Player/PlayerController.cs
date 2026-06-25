using ProjectEclipse.Combat;
using ProjectEclipse.Equipment;
using ProjectEclipse.Utilities;
using ProjectEclipse.World;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEclipse.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float jumpForce = 15.2f;
        [SerializeField] private float acceleration = 58f;
        [SerializeField] private float deceleration = 72f;
        [SerializeField] private float airControl = 0.62f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField] private float lowJumpGravityMultiplier = 2.1f;
        [SerializeField] private float fallGravityMultiplier = 2.4f;
        [SerializeField] private float oneWayIgnoreSeconds = 0.18f;
        [SerializeField] private float dropThroughSeconds = 0.34f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private PlayerClassDefinition classDefinition;

        private class IgnoredPlatformCollision
        {
            public Collider2D Collider;
            public float ExpiresAt;
        }

        private readonly List<IgnoredPlatformCollision> ignoredPlatforms = new List<IgnoredPlatformCollision>();
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private CombatController combat;
        private CombatInputRouter combatInput;
        private EquipmentController equipment;
        private VisualStateAnimator visualState;
        private PlayerRespawnController respawnController;
        private int facingDirection = 1;
        private bool grounded;
        private Collider2D groundedCollider;
        private float horizontalInput;
        private float lastGroundedTime;
        private float jumpBufferExpiresAt;
        private bool jumpHeld;

        public int FacingDirection { get { return facingDirection; } }
        public bool IsGrounded { get { return grounded; } }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            combat = GetComponent<CombatController>();
            combatInput = GetComponent<CombatInputRouter>();
            equipment = GetComponent<EquipmentController>();
            visualState = GetComponent<VisualStateAnimator>();
            respawnController = GetComponent<PlayerRespawnController>();
            ApplyClassMovementDefaults();
        }

        private void Update()
        {
            ExpireIgnoredPlatforms();
            grounded = CheckGrounded(out groundedCollider);
            if (IsInputBlocked())
            {
                horizontalInput = 0f;
                jumpBufferExpiresAt = 0f;
                jumpHeld = false;
                if (visualState != null)
                {
                    visualState.SetMoving(false);
                    visualState.SetGrounded(grounded);
                }

                return;
            }

            if (grounded)
            {
                lastGroundedTime = Time.time;
            }
            horizontalInput = ReadHorizontalInput();
            bool jumpPressed = WantsJump();
            if (WantsDropThrough(jumpPressed))
            {
                DropThroughCurrentPlatform();
                jumpPressed = false;
            }

            if (jumpPressed)
            {
                jumpBufferExpiresAt = Time.time + jumpBufferTime;
            }
            jumpHeld = WantsJumpHeld();

            UpdateFacing(horizontalInput);

            if (combatInput != null)
            {
                int requestedFacing = combatInput.PollActions(facingDirection);
                if (requestedFacing != 0)
                {
                    SetFacingDirection(requestedFacing);
                }
            }
            else if (WantsAttack() && combat != null)
            {
                combat.TryAttack(facingDirection);
            }

            if (visualState != null)
            {
                visualState.SetMoving(Mathf.Abs(horizontalInput) > 0.01f);
                visualState.SetGrounded(grounded);
            }
        }

        private void FixedUpdate()
        {
            ExpireIgnoredPlatforms();
            if (IsInputBlocked())
            {
                if (body != null && body.simulated)
                {
                    body.linearVelocity = Vector2.zero;
                }

                return;
            }

            Vector2 velocity = body.linearVelocity;
            float speedMultiplier = combatInput != null && combatInput.SprintHeld ? combatInput.SprintSpeedMultiplier : 1f;
            float targetSpeed = horizontalInput * GetEffectiveMoveSpeed() * speedMultiplier;
            float control = grounded ? 1f : GetEffectiveAirControl();
            float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, rate * control * Time.fixedDeltaTime);
            body.linearVelocity = velocity;

            if (CanConsumeJump())
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, GetEffectiveJumpForce());
                jumpBufferExpiresAt = 0f;
                lastGroundedTime = 0f;
            }

            ApplyBetterGravity();
        }

        private void UpdateFacing(float horizontal)
        {
            if (Mathf.Abs(horizontal) > 0.01f)
            {
                SetFacingDirection(horizontal > 0f ? 1 : -1);
            }
        }

        private bool IsInputBlocked()
        {
            if (respawnController == null)
            {
                respawnController = GetComponent<PlayerRespawnController>();
            }

            return respawnController != null && respawnController.BlocksPlayerInput;
        }

        private void SetFacingDirection(int direction)
        {
            facingDirection = direction >= 0 ? 1 : -1;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            transform.localScale = scale;
            if (equipment != null)
            {
                equipment.SetFacingDirection(facingDirection);
            }
        }

        private void ApplyClassMovementDefaults()
        {
            if (classDefinition == null)
            {
                return;
            }

            moveSpeed = classDefinition.MoveSpeed;
            jumpForce = classDefinition.JumpForce;
        }

        private float ReadHorizontalInput()
        {
            float horizontal = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                horizontal -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                horizontal += 1f;
            }

            return Mathf.Clamp(horizontal, -1f, 1f);
        }

        private bool WantsJump()
        {
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        }

        private bool WantsJumpHeld()
        {
            return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        private bool WantsDropHeld()
        {
            return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        }

        private bool WantsDropThrough(bool jumpPressed)
        {
            return jumpPressed && grounded && WantsDropHeld() && IsOneWayPlatform(groundedCollider);
        }

        private bool CanConsumeJump()
        {
            return Time.time <= jumpBufferExpiresAt && Time.time - lastGroundedTime <= coyoteTime;
        }

        private void ApplyBetterGravity()
        {
            if (body.linearVelocity.y < -0.01f)
            {
                body.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime);
            }
            else if (body.linearVelocity.y > 0.01f && !jumpHeld)
            {
                body.linearVelocity += Vector2.up * (Physics2D.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.fixedDeltaTime);
            }
        }

        private float GetEffectiveMoveSpeed()
        {
            float bonus = equipment != null ? equipment.TotalMoveSpeedBonus : 0f;
            return Mathf.Max(0.1f, moveSpeed + bonus);
        }

        private float GetEffectiveJumpForce()
        {
            float bonus = equipment != null ? equipment.TotalJumpForceBonus : 0f;
            return Mathf.Max(0.1f, jumpForce + bonus);
        }

        private float GetEffectiveAirControl()
        {
            float bonus = equipment != null ? equipment.TotalAirControlBonus : 0f;
            return Mathf.Max(0.05f, airControl + bonus);
        }

        private bool WantsAttack()
        {
            return Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftControl);
        }

        private bool CheckGrounded(out Collider2D groundCollider)
        {
            groundCollider = null;
            Bounds bounds = bodyCollider.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.03f);
            Vector2 size = new Vector2(bounds.size.x * 0.88f, 0.05f);
            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0f, Vector2.down, 0.08f, groundMask);

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hitCollider = hits[i].collider;
                if (IsValidGroundHit(hitCollider))
                {
                    groundCollider = hitCollider;
                    return true;
                }
            }

            return false;
        }

        private bool IsValidGroundHit(Collider2D hitCollider)
        {
            if (hitCollider == null || hitCollider == bodyCollider || hitCollider.isTrigger)
            {
                return false;
            }

            if (Physics2D.GetIgnoreCollision(bodyCollider, hitCollider))
            {
                return false;
            }

            if (IsOneWayPlatform(hitCollider) && ShouldIgnoreOneWayPlatform(hitCollider))
            {
                IgnorePlatformTemporarily(hitCollider, oneWayIgnoreSeconds);
                return false;
            }

            return true;
        }

        private void DropThroughCurrentPlatform()
        {
            if (!IsOneWayPlatform(groundedCollider))
            {
                return;
            }

            IgnorePlatformTemporarily(groundedCollider, dropThroughSeconds);
            grounded = false;
            groundedCollider = null;
            lastGroundedTime = 0f;
            jumpBufferExpiresAt = 0f;
            body.linearVelocity = new Vector2(body.linearVelocity.x, Mathf.Min(body.linearVelocity.y, -0.75f));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryIgnoreOneWayCollision(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryIgnoreOneWayCollision(collision);
        }

        private void TryIgnoreOneWayCollision(Collision2D collision)
        {
            if (collision == null || collision.collider == null || !IsOneWayPlatform(collision.collider))
            {
                return;
            }

            if (ShouldIgnoreOneWayPlatform(collision.collider))
            {
                IgnorePlatformTemporarily(collision.collider, oneWayIgnoreSeconds);
            }
        }

        private bool ShouldIgnoreOneWayPlatform(Collider2D platformCollider)
        {
            if (platformCollider == null || bodyCollider == null)
            {
                return false;
            }

            OneWayPlatform oneWay = platformCollider.GetComponent<OneWayPlatform>();
            if (oneWay == null)
            {
                return false;
            }

            float platformTop = platformCollider.bounds.max.y;
            float playerBottom = bodyCollider.bounds.min.y;
            return body.linearVelocity.y > 0.05f || playerBottom < platformTop - oneWay.SurfaceTolerance;
        }

        private static bool IsOneWayPlatform(Collider2D platformCollider)
        {
            return platformCollider != null && platformCollider.GetComponent<OneWayPlatform>() != null;
        }

        private void IgnorePlatformTemporarily(Collider2D platformCollider, float seconds)
        {
            if (platformCollider == null || bodyCollider == null)
            {
                return;
            }

            Physics2D.IgnoreCollision(bodyCollider, platformCollider, true);
            float expiresAt = Time.time + Mathf.Max(0.05f, seconds);
            for (int i = 0; i < ignoredPlatforms.Count; i++)
            {
                if (ignoredPlatforms[i] != null && ignoredPlatforms[i].Collider == platformCollider)
                {
                    ignoredPlatforms[i].ExpiresAt = Mathf.Max(ignoredPlatforms[i].ExpiresAt, expiresAt);
                    return;
                }
            }

            ignoredPlatforms.Add(new IgnoredPlatformCollision
            {
                Collider = platformCollider,
                ExpiresAt = expiresAt
            });
        }

        private void ExpireIgnoredPlatforms()
        {
            for (int i = ignoredPlatforms.Count - 1; i >= 0; i--)
            {
                IgnoredPlatformCollision ignored = ignoredPlatforms[i];
                if (ignored == null || ignored.Collider == null || bodyCollider == null)
                {
                    ignoredPlatforms.RemoveAt(i);
                    continue;
                }

                if (Time.time < ignored.ExpiresAt || ShouldIgnoreOneWayPlatform(ignored.Collider))
                {
                    continue;
                }

                Physics2D.IgnoreCollision(bodyCollider, ignored.Collider, false);
                ignoredPlatforms.RemoveAt(i);
            }
        }

        private void OnDisable()
        {
            for (int i = ignoredPlatforms.Count - 1; i >= 0; i--)
            {
                if (ignoredPlatforms[i] != null && ignoredPlatforms[i].Collider != null && bodyCollider != null)
                {
                    Physics2D.IgnoreCollision(bodyCollider, ignoredPlatforms[i].Collider, false);
                }
            }

            ignoredPlatforms.Clear();
        }
    }
}
