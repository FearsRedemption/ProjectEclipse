using System.Collections.Generic;
using ProjectEclipse.Enemies;
using UnityEngine;

namespace ProjectEclipse.Utilities
{
    public class SpriteSheetAnimator : MonoBehaviour
    {
        private const int IdleRow = 0;
        private const int MoveRow = 1;
        private const int AttackRow = 2;
        private const int HurtRow = 3;
        private const int DieRow = 4;

        private static readonly int[] DefaultFrameCounts = { 4, 6, 6, 2, 6 };

        [SerializeField] private float idleFps = 6f;
        [SerializeField] private float moveFps = 10f;
        [SerializeField] private float attackFps = 14f;
        [SerializeField] private float hurtFps = 12f;
        [SerializeField] private float dieFps = 9f;

        private readonly Dictionary<EnemyState, Sprite[]> clips = new Dictionary<EnemyState, Sprite[]>();
        private SpriteRenderer spriteRenderer;
        private EnemyState state = EnemyState.Idle;
        private bool moving;
        private bool oneShot;
        private bool dead;
        private float timer;
        private int frameIndex;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            Sprite[] frames;
            if (!clips.TryGetValue(state, out frames) || frames == null || frames.Length == 0)
            {
                return;
            }

            timer += Time.deltaTime;
            float frameDuration = 1f / GetFps(state);
            if (timer < frameDuration)
            {
                return;
            }

            timer -= frameDuration;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                if (dead)
                {
                    frameIndex = frames.Length - 1;
                }
                else if (oneShot)
                {
                    oneShot = false;
                    SetLoopingState(moving ? EnemyState.Moving : EnemyState.Idle);
                    return;
                }
                else
                {
                    frameIndex = 0;
                }
            }

            ApplyFrame();
        }

        public void Configure(Texture2D sheet, int frameWidth, int frameHeight, float pixelsPerUnit)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            clips.Clear();
            if (sheet == null)
            {
                return;
            }

            clips[EnemyState.Idle] = SliceRow(sheet, IdleRow, DefaultFrameCounts[IdleRow], frameWidth, frameHeight, pixelsPerUnit);
            clips[EnemyState.Moving] = SliceRow(sheet, MoveRow, DefaultFrameCounts[MoveRow], frameWidth, frameHeight, pixelsPerUnit);
            clips[EnemyState.Attacking] = SliceRow(sheet, AttackRow, DefaultFrameCounts[AttackRow], frameWidth, frameHeight, pixelsPerUnit);
            clips[EnemyState.Hurt] = SliceRow(sheet, HurtRow, DefaultFrameCounts[HurtRow], frameWidth, frameHeight, pixelsPerUnit);
            clips[EnemyState.Dying] = SliceRow(sheet, DieRow, DefaultFrameCounts[DieRow], frameWidth, frameHeight, pixelsPerUnit);
            SetLoopingState(EnemyState.Idle);
        }

        public void SetMoving(bool value)
        {
            moving = value;
            if (dead || oneShot)
            {
                return;
            }

            SetLoopingState(value ? EnemyState.Moving : EnemyState.Idle);
        }

        public void TriggerAttack()
        {
            SetOneShotState(EnemyState.Attacking);
        }

        public void TriggerHurt()
        {
            if (!dead)
            {
                SetOneShotState(EnemyState.Hurt);
            }
        }

        public void TriggerDie()
        {
            dead = true;
            oneShot = false;
            SetLoopingState(EnemyState.Dying);
        }

        private Sprite[] SliceRow(Texture2D sheet, int row, int count, int frameWidth, int frameHeight, float pixelsPerUnit)
        {
            Sprite[] frames = new Sprite[count];
            int y = sheet.height - ((row + 1) * frameHeight);
            for (int i = 0; i < count; i++)
            {
                Rect rect = new Rect(i * frameWidth, y, frameWidth, frameHeight);
                frames[i] = Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
                frames[i].name = sheet.name + "_" + row + "_" + i;
            }

            return frames;
        }

        private void SetLoopingState(EnemyState nextState)
        {
            if (state == nextState && !oneShot)
            {
                return;
            }

            state = nextState;
            oneShot = false;
            frameIndex = 0;
            timer = 0f;
            ApplyFrame();
        }

        private void SetOneShotState(EnemyState nextState)
        {
            if (dead)
            {
                return;
            }

            state = nextState;
            oneShot = true;
            frameIndex = 0;
            timer = 0f;
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            Sprite[] frames;
            if (spriteRenderer == null || !clips.TryGetValue(state, out frames) || frames == null || frames.Length == 0)
            {
                return;
            }

            spriteRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        }

        private float GetFps(EnemyState clipState)
        {
            switch (clipState)
            {
                case EnemyState.Moving:
                    return moveFps;
                case EnemyState.Attacking:
                    return attackFps;
                case EnemyState.Hurt:
                    return hurtFps;
                case EnemyState.Dying:
                    return dieFps;
                default:
                    return idleFps;
            }
        }
    }
}
