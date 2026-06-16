using ProjectEclipse.Combat;
using ProjectEclipse.Equipment;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float jumpForce = 13f;
        [SerializeField] private float acceleration = 58f;
        [SerializeField] private float deceleration = 72f;
        [SerializeField] private float airControl = 0.62f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField] private float lowJumpGravityMultiplier = 2.1f;
        [SerializeField] private float fallGravityMultiplier = 2.4f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private PlayerClassDefinition classDefinition;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private CombatController combat;
        private CombatInputRouter combatInput;
        private EquipmentController equipment;
        private VisualStateAnimator visualState;
        private int facingDirection = 1;
        private bool grounded;
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
            ApplyClassMovementDefaults();
        }

        private void Update()
        {
            grounded = CheckGrounded();
            if (grounded)
            {
                lastGroundedTime = Time.time;
            }
            horizontalInput = ReadHorizontalInput();
            if (WantsJump())
            {
                jumpBufferExpiresAt = Time.time + jumpBufferTime;
            }
            jumpHeld = WantsJumpHeld();

            UpdateFacing(horizontalInput);

            if (combatInput != null)
            {
                combatInput.PollActions(facingDirection);
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
            Vector2 velocity = body.linearVelocity;
            float speedMultiplier = combatInput != null && combatInput.SprintHeld ? combatInput.SprintSpeedMultiplier : 1f;
            float targetSpeed = horizontalInput * moveSpeed * speedMultiplier;
            float control = grounded ? 1f : airControl;
            float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, rate * control * Time.fixedDeltaTime);
            body.linearVelocity = velocity;

            if (CanConsumeJump())
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce + GetBackJumpBonus());
                jumpBufferExpiresAt = 0f;
                lastGroundedTime = 0f;
            }

            ApplyBetterGravity();
        }

        private void UpdateFacing(float horizontal)
        {
            if (Mathf.Abs(horizontal) > 0.01f)
            {
                facingDirection = horizontal > 0f ? 1 : -1;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * facingDirection;
                transform.localScale = scale;
                if (equipment != null)
                {
                    equipment.SetFacingDirection(facingDirection);
                }
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

        private float GetBackJumpBonus()
        {
            if (equipment == null || equipment.Back == null)
            {
                return 0f;
            }

            return equipment.Back.Stats.JumpForceBonus;
        }

        private bool WantsAttack()
        {
            return Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftControl);
        }

        private bool CheckGrounded()
        {
            Bounds bounds = bodyCollider.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.03f);
            Vector2 size = new Vector2(bounds.size.x * 0.88f, 0.05f);
            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0f, Vector2.down, 0.08f, groundMask);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null && hits[i].collider != bodyCollider && !hits[i].collider.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
