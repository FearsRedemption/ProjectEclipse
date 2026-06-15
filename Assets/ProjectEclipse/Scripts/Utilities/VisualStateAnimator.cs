using UnityEngine;

namespace ProjectEclipse.Utilities
{
    public class VisualStateAnimator : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DieHash = Animator.StringToHash("Die");

        [SerializeField] private float bobAmount = 0.045f;
        [SerializeField] private float bobSpeed = 8f;

        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Vector3 baseScale;
        private Color baseColor;
        private bool moving;
        private bool dead;
        private float attackPulseUntil;
        private float hurtPulseUntil;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
            baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        }

        private void Update()
        {
            if (dead)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(baseScale.x, 0.08f, baseScale.z), Time.deltaTime * 10f);
                if (spriteRenderer != null)
                {
                    Color faded = spriteRenderer.color;
                    faded.a = Mathf.MoveTowards(faded.a, 0.35f, Time.deltaTime * 4f);
                    spriteRenderer.color = faded;
                }
                return;
            }

            float bob = moving ? Mathf.Sin(Time.time * bobSpeed) * bobAmount : 0f;
            float attackScale = Time.time < attackPulseUntil ? 1.12f : 1f;
            transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(baseScale.x) * attackScale, baseScale.y + bob, baseScale.z);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Time.time < hurtPulseUntil ? Color.white : baseColor;
            }
        }

        public void SetBaseColor(Color color)
        {
            baseColor = color;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        public void SetMoving(bool value)
        {
            moving = value;
            if (animator != null)
            {
                animator.SetBool(IsMovingHash, value);
            }
        }

        public void SetGrounded(bool value)
        {
            if (animator != null)
            {
                animator.SetBool(IsGroundedHash, value);
            }
        }

        public void TriggerAttack()
        {
            attackPulseUntil = Time.time + 0.12f;
            if (animator != null)
            {
                animator.SetTrigger(AttackHash);
            }
        }

        public void TriggerHurt()
        {
            hurtPulseUntil = Time.time + 0.11f;
            if (animator != null)
            {
                animator.SetTrigger(HurtHash);
            }
        }

        public void TriggerDie()
        {
            dead = true;
            if (animator != null)
            {
                animator.SetTrigger(DieHash);
            }
        }
    }
}

