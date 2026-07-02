using UnityEngine;

namespace ProjectEclipse.World
{
    public class PortalVisualAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Texture2D spriteSheet;
        [SerializeField] private int frameWidth = 96;
        [SerializeField] private int frameHeight = 144;
        [SerializeField] private float pixelsPerUnit = 96f;
        [SerializeField] private float framesPerSecond = 10f;
        [SerializeField] private float pulseStrength = 0.035f;
        [SerializeField] private Color baseColor = Color.white;

        private Sprite[] frames;
        private Vector3 baseScale = Vector3.one;
        private float timer;
        private int frameIndex;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            baseScale = transform.localScale;
            RebuildFrames();
            ApplyFrame();
        }

        private void Update()
        {
            if (frames != null && frames.Length > 0 && framesPerSecond > 0.01f)
            {
                timer += Time.deltaTime;
                float frameDuration = 1f / framesPerSecond;
                while (timer >= frameDuration)
                {
                    timer -= frameDuration;
                    frameIndex = (frameIndex + 1) % frames.Length;
                    ApplyFrame();
                }
            }

            float pulse = Mathf.Sin(Time.time * 5.1f) * 0.5f + 0.5f;
            float scale = 1f + pulseStrength * pulse;
            transform.localScale = new Vector3(baseScale.x * scale, baseScale.y * (1f + pulseStrength * 0.55f * pulse), baseScale.z);

            if (targetRenderer != null)
            {
                Color color = baseColor;
                color.a *= Mathf.Lerp(0.84f, 1f, pulse);
                targetRenderer.color = color;
            }
        }

        public void Configure(Texture2D sheet, int width, int height, float ppu, float fps, Color color)
        {
            spriteSheet = sheet;
            frameWidth = Mathf.Max(1, width);
            frameHeight = Mathf.Max(1, height);
            pixelsPerUnit = Mathf.Max(1f, ppu);
            framesPerSecond = Mathf.Max(0.01f, fps);
            baseColor = color;

            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            if (targetRenderer != null)
            {
                targetRenderer.color = baseColor;
            }

            RebuildFrames();
            ApplyFrame();
        }

        private void RebuildFrames()
        {
            frames = null;
            if (spriteSheet == null || frameWidth <= 0 || frameHeight <= 0)
            {
                return;
            }

            int frameCount = Mathf.Max(1, spriteSheet.width / frameWidth);
            frames = new Sprite[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                Rect rect = new Rect(i * frameWidth, 0f, frameWidth, Mathf.Min(frameHeight, spriteSheet.height));
                frames[i] = Sprite.Create(spriteSheet, rect, new Vector2(0.5f, 0.06f), pixelsPerUnit);
                frames[i].name = spriteSheet.name + "_portal_" + i.ToString();
            }
        }

        private void ApplyFrame()
        {
            if (targetRenderer == null || frames == null || frames.Length == 0)
            {
                return;
            }

            targetRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        }
    }
}
