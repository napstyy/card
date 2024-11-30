using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace CardGame
{
    public class CardAnimationSystem : MonoBehaviour
    {
        [Header("Draw Animation Settings")]
        [SerializeField] private float drawDuration = 0.6f;
        [SerializeField] private float arcHeight = 1.5f;
        [SerializeField]
        private AnimationCurve drawCurve = new AnimationCurve(
            new Keyframe(0, 0, 0, 1),    // Start: slow
            new Keyframe(0.4f, 0.8f, 1, 1),  // Middle: fast
            new Keyframe(1, 1, 0.5f, 0)  // End: ease out
        );

        [Header("Flip Animation")]
        [SerializeField] private float flipDuration = 0.4f;
        [SerializeField]
        private AnimationCurve flipCurve = new AnimationCurve(
            new Keyframe(0, 0, 0, 2),
            new Keyframe(0.5f, 1, 2, 2),
            new Keyframe(1, 0, 2, 0)
        );

        [Header("Scale Effects")]
        [SerializeField] private float scaleUpAmount = 1.1f;
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        public static CardAnimationSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void AnimateCardDraw(GameObject cardObject, Vector3 startPosition, Vector3 targetPosition, bool faceUp = true)
        {
            if (cardObject == null) return;

            DOTween.Kill(cardObject.transform);

            // Initial setup
            cardObject.transform.position = startPosition;
            cardObject.transform.rotation = Quaternion.identity;
            cardObject.transform.localScale = Vector3.one;

            Sequence drawSequence = DOTween.Sequence();

            // Calculate better arc points for more natural movement
            Vector3 midPoint = (startPosition + targetPosition) * 0.5f;
            float horizontalDistance = Vector3.Distance(startPosition, targetPosition);
            float dynamicArcHeight = Mathf.Min(arcHeight, horizontalDistance * 0.4f);

            Vector3 controlPoint1 = Vector3.Lerp(startPosition, midPoint, 0.25f) + Vector3.up * (dynamicArcHeight * 0.75f);
            Vector3 controlPoint2 = Vector3.Lerp(midPoint, targetPosition, 0.75f) + Vector3.up * (dynamicArcHeight * 0.25f);

            Vector3[] pathPoints = new Vector3[] {
                startPosition,
                controlPoint1,
                midPoint + Vector3.up * dynamicArcHeight,
                controlPoint2,
                targetPosition
            };

            // Smooth path movement
            drawSequence.Append(cardObject.transform
                .DOPath(pathPoints, drawDuration, PathType.CatmullRom)
                .SetEase(Ease.OutQuint));

            // Enhanced rotation with slight wobble
            if (faceUp)
            {
                Sequence rotationSequence = DOTween.Sequence();

                // Initial slight tilt
                rotationSequence.Append(cardObject.transform
                    .DORotate(new Vector3(0, 15, 0), drawDuration * 0.2f)
                    .SetEase(Ease.OutQuad));

                // Main rotation
                rotationSequence.Append(cardObject.transform
                    .DORotate(new Vector3(0, 180, 0), drawDuration * 0.6f)
                    .SetEase(flipCurve));

                // Final stabilization
                rotationSequence.Append(cardObject.transform
                    .DORotate(new Vector3(0, 180, 0), drawDuration * 0.2f)
                    .SetEase(Ease.OutBack));

                drawSequence.Join(rotationSequence);
            }

            // Dynamic scale effect
            Sequence scaleSequence = DOTween.Sequence();

            // Slight scale up during arc
            scaleSequence.Append(cardObject.transform
                .DOScale(Vector3.one * scaleUpAmount, drawDuration * 0.4f)
                .SetEase(scaleEase));

            // Return to normal with bounce
            scaleSequence.Append(cardObject.transform
                .DOScale(Vector3.one, drawDuration * 0.6f)
                .SetEase(Ease.OutBack));

            drawSequence.Join(scaleSequence);

            // Optional: Add subtle rotation around Z axis for more natural feel
            if (faceUp)
            {
                drawSequence.Join(cardObject.transform
                    .DORotate(new Vector3(0, 180, Random.Range(-2f, 2f)), drawDuration)
                    .SetEase(Ease.OutQuad));
            }

            drawSequence.Play();
        }

        public void AnimateCardFlip(GameObject cardObject, bool faceUp)
        {
            if (cardObject == null) return;

            DOTween.Kill(cardObject.transform);
            Sequence flipSequence = DOTween.Sequence();

            // Add slight upward movement during flip
            flipSequence.Join(cardObject.transform
                .DOLocalMoveY(cardObject.transform.localPosition.y + 0.1f, flipDuration * 0.5f)
                .SetLoops(2, LoopType.Yoyo));

            // Enhanced flip animation
            flipSequence.Join(cardObject.transform
                .DORotate(new Vector3(0, 90, 0), flipDuration * 0.5f)
                .SetEase(flipCurve)
                .OnComplete(() =>
                {
                    DisplayCard displayCard = cardObject.GetComponent<DisplayCard>();
                    if (displayCard != null)
                    {
                        if (faceUp) displayCard.ShowCard();
                        else displayCard.HideCard();
                    }
                }));

            flipSequence.Append(cardObject.transform
                .DORotate(new Vector3(0, faceUp ? 180 : 0, 0), flipDuration * 0.5f)
                .SetEase(flipCurve));

            // Add subtle scale effect during flip
            flipSequence.Join(cardObject.transform
                .DOScale(Vector3.one * 1.05f, flipDuration)
                .SetEase(flipCurve)
                .SetLoops(2, LoopType.Yoyo));

            flipSequence.Play();
        }

        public void AnimateCardMove(GameObject cardObject, Vector3 targetPosition, float duration = 0.3f)
        {
            if (cardObject == null) return;

            DOTween.Kill(cardObject.transform);

            Sequence moveSequence = DOTween.Sequence();

            // Slight arc movement
            float distance = Vector3.Distance(cardObject.transform.position, targetPosition);
            float arcHeight = distance * 0.1f;
            Vector3 midPoint = Vector3.Lerp(cardObject.transform.position, targetPosition, 0.5f) + Vector3.up * arcHeight;

            moveSequence.Append(cardObject.transform
                .DOPath(new Vector3[] {
                    cardObject.transform.position,
                    midPoint,
                    targetPosition
                }, duration, PathType.CatmullRom)
                .SetEase(Ease.OutQuint));

            // Subtle rotation during movement
            moveSequence.Join(cardObject.transform
                .DORotate(new Vector3(0, cardObject.transform.rotation.eulerAngles.y, Random.Range(-1f, 1f)), duration)
                .SetEase(Ease.OutQuad));

            moveSequence.Play();
        }
    }
}