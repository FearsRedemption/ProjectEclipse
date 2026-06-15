using ProjectEclipse.Combat;
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
        [SerializeField] private LayerMask groundMask = ~0;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private CombatController combat;
        private VisualStateAnimator visualState;
        private int facingDirection = 1;
        private bool grounded;

        public int FacingDirection { get { return facingDirection; } }
        public bool IsGrounded { get { return grounded; } }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            combat = GetComponent<CombatController>();
            visualState = GetComponent<VisualStateAnimator>();
        }

        private void Update()
        {
            grounded = CheckGrounded();
            float horizontal = ReadHorizontalInput();
            Vector2 velocity = body.linearVelocity;
            velocity.x = horizontal * moveSpeed;
            body.linearVelocity = velocity;

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                facingDirection = horizontal > 0f ? 1 : -1;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * facingDirection;
                transform.localScale = scale;
            }

            if (grounded && WantsJump())
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            }

            if (WantsAttack() && combat != null)
            {
                combat.TryAttack(facingDirection);
            }

            if (visualState != null)
            {
                visualState.SetMoving(Mathf.Abs(horizontal) > 0.01f);
                visualState.SetGrounded(grounded);
            }
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

