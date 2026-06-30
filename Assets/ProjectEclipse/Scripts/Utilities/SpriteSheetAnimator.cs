using System.Collections.Generic;
using ProjectEclipse.Enemies;
using UnityEngine;

namespace ProjectEclipse.Utilities
{
    public class SpriteSheetAnimator : MonoBehaviour
    {
        [Header("Sheet")]
        [SerializeField] private Texture2D spriteSheet;
        [SerializeField] private int frameWidth = 96;
        [SerializeField] private int frameHeight = 96;
        [SerializeField] private float pixelsPerUnit = 96f;
#pragma warning disable CS0649
        [SerializeField] private bool configureOnAwake;
#pragma warning restore CS0649

        [Header("Rows")]
        [SerializeField] private int idleRow = 0;
        [SerializeField] private int moveRow = 1;
        [SerializeField] private int airborneRow = -1;
        [SerializeField] private int attackRow = 2;
        [SerializeField] private int hurtRow = 3;
        [SerializeField] private int dieRow = 4;

        [Header("Frame Counts")]
        [SerializeField] private bool detectFrameCountsFromSheet = true;
        [SerializeField] private byte visibleAlphaThreshold = 8;
        [SerializeField] private int idleFrameCount = 4;
        [SerializeField] private int moveFrameCount = 6;
        [SerializeField] private int airborneFrameCount = 2;
        [SerializeField] private int attackFrameCount = 6;
        [SerializeField] private int hurtFrameCount = 2;
        [SerializeField] private int dieFrameCount = 6;

        [Header("Timing")]
        [SerializeField] private float idleFps = 6f;
        [SerializeField] private float moveFps = 10f;
        [SerializeField] private float airborneFps = 8f;
        [SerializeField] private float attackFps = 14f;
        [SerializeField] private float hurtFps = 12f;
        [SerializeField] private float dieFps = 9f;

        private readonly Dictionary<EnemyState, Sprite[]> clips = new Dictionary<EnemyState, Sprite[]>();
        private SpriteRenderer spriteRenderer;
        private EnemyState state = EnemyState.Idle;
        private bool moving;
        private bool grounded = true;
        private bool oneShot;
        private bool dead;
        private float timer;
        private int frameIndex;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (configureOnAwake && spriteSheet != null)
            {
                RebuildClips();
            }
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
                    SetLoopingState(GetDefaultLoopingState());
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
            spriteSheet = sheet;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.pixelsPerUnit = pixelsPerUnit;
            RebuildClips();
        }

        public void SetMoving(bool value)
        {
            moving = value;
            if (dead || oneShot)
            {
                return;
            }

            SetLoopingState(GetDefaultLoopingState());
        }

        public void SetGrounded(bool value)
        {
            grounded = value;
            if (dead || oneShot)
            {
                return;
            }

            SetLoopingState(GetDefaultLoopingState());
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

        public void ResetToIdle()
        {
            dead = false;
            oneShot = false;
            moving = false;
            grounded = true;
            SetLoopingState(EnemyState.Idle);
        }

        private void RebuildClips()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            clips.Clear();
            if (spriteSheet == null)
            {
                return;
            }

            AddClip(EnemyState.Idle, idleRow, idleFrameCount);
            AddClip(EnemyState.Moving, moveRow, moveFrameCount);
            AddClip(EnemyState.Airborne, airborneRow, airborneFrameCount);
            AddClip(EnemyState.Attacking, attackRow, attackFrameCount);
            AddClip(EnemyState.Hurt, hurtRow, hurtFrameCount);
            AddClip(EnemyState.Dying, dieRow, dieFrameCount);
            SetLoopingState(EnemyState.Idle);
        }

        private void AddClip(EnemyState clipState, int row, int count)
        {
            if (row < 0 || count <= 0 || spriteSheet == null)
            {
                return;
            }

            if ((row + 1) * frameHeight > spriteSheet.height)
            {
                return;
            }

            int resolvedCount = ResolveFrameCount(row, count);
            if (resolvedCount <= 0)
            {
                return;
            }

            clips[clipState] = SliceRow(row, resolvedCount);
        }

        private int ResolveFrameCount(int row, int configuredCount)
        {
            int cellsInRow = Mathf.FloorToInt(spriteSheet.width / (float)frameWidth);
            int fallbackCount = Mathf.Clamp(configuredCount, 0, cellsInRow);
            if (!detectFrameCountsFromSheet)
            {
                return fallbackCount;
            }

            int detectedCount = DetectNonEmptyFrameCount(row, cellsInRow);
            return detectedCount > 0 ? detectedCount : fallbackCount;
        }

        private int DetectNonEmptyFrameCount(int row, int cellsInRow)
        {
            int y = spriteSheet.height - ((row + 1) * frameHeight);
            int detectedCount = 0;

            try
            {
                Color32[] pixels = spriteSheet.GetPixels32();
                for (int frame = 0; frame < cellsInRow; frame++)
                {
                    if (FrameHasVisiblePixels(pixels, frame * frameWidth, y))
                    {
                        detectedCount = frame + 1;
                    }
                }
            }
            catch (UnityException)
            {
                return 0;
            }

            return detectedCount;
        }

        private bool FrameHasVisiblePixels(Color32[] pixels, int x, int y)
        {
            int textureWidth = spriteSheet.width;
            int maxX = Mathf.Min(x + frameWidth, spriteSheet.width);
            int maxY = Mathf.Min(y + frameHeight, spriteSheet.height);

            for (int py = y; py < maxY; py++)
            {
                int rowOffset = py * textureWidth;
                for (int px = x; px < maxX; px++)
                {
                    if (pixels[rowOffset + px].a > visibleAlphaThreshold)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Sprite[] SliceRow(int row, int count)
        {
            Sprite[] frames = new Sprite[count];
            int y = spriteSheet.height - ((row + 1) * frameHeight);
            for (int i = 0; i < count; i++)
            {
                Rect rect = new Rect(i * frameWidth, y, frameWidth, frameHeight);
                frames[i] = Sprite.Create(spriteSheet, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
                frames[i].name = spriteSheet.name + "_" + row + "_" + i;
            }

            return frames;
        }

        private void SetLoopingState(EnemyState nextState)
        {
            if (!clips.ContainsKey(nextState))
            {
                nextState = moving && clips.ContainsKey(EnemyState.Moving) ? EnemyState.Moving : EnemyState.Idle;
            }

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
            if (dead || !clips.ContainsKey(nextState))
            {
                return;
            }

            state = nextState;
            oneShot = true;
            frameIndex = 0;
            timer = 0f;
            ApplyFrame();
        }

        private EnemyState GetDefaultLoopingState()
        {
            if (!grounded && clips.ContainsKey(EnemyState.Airborne))
            {
                return EnemyState.Airborne;
            }

            return moving ? EnemyState.Moving : EnemyState.Idle;
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
                case EnemyState.Airborne:
                    return airborneFps;
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
