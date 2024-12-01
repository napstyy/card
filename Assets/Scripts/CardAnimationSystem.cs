using UnityEngine;
using DG.Tweening;

namespace CardGame
{
    public class CardAnimationSystem : MonoBehaviour
    {
        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 1.2f;
        [SerializeField] private Vector2 dealerHandPosition = new Vector2(0, 2f); // Center, top
        [SerializeField] private Vector2 playerHandPosition = new Vector2(0, -2f); // Center, bottom

        [Header("Animation Settings")]
        [SerializeField] private float drawDuration = 0.6f;
        [SerializeField] private float arcHeight = 1.5f;

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

        public void AnimateCardDraw(GameObject cardObject, Transform handTransform, int cardIndex, int totalCards, bool isDealerHand, bool faceUp = true)
        {
            if (cardObject == null || handTransform == null) return;

            // Calculate center point for the hand
            Vector2 handCenter = isDealerHand ? dealerHandPosition : playerHandPosition;
            float totalWidth = (totalCards - 1) * cardSpacing;
            float startX = -totalWidth / 2f;

            // Calculate target position (centered)
            Vector3 targetPosition = new Vector3(
                startX + (cardIndex * cardSpacing),
                handCenter.y,
                0
            );

            // Start position (off-screen right)
            Vector3 startPosition = new Vector3(
                targetPosition.x + 10f,
                targetPosition.y,
                0
            );

            // Setup initial card state
            cardObject.transform.position = startPosition;
            cardObject.transform.rotation = Quaternion.identity;
            cardObject.transform.localScale = Vector3.one;

            // Create animation sequence
            Sequence drawSequence = DOTween.Sequence();

            // Path movement with arc
            Vector3[] pathPoints = CalculateArcPath(startPosition, targetPosition);
            drawSequence.Append(cardObject.transform
                .DOPath(pathPoints, drawDuration, PathType.CatmullRom)
                .SetEase(Ease.OutQuad));

            // Card flip animation
            if (faceUp)
            {
                drawSequence.Join(cardObject.transform
                    .DORotate(new Vector3(0, 180, 0), drawDuration)
                    .SetEase(Ease.OutQuad));
            }

            drawSequence.Play();
        }

        private Vector3[] CalculateArcPath(Vector3 start, Vector3 end)
        {
            Vector3 midPoint = (start + end) * 0.5f;
            float height = arcHeight;

            return new Vector3[]
            {
                start,
                start + (Vector3.up * height * 0.5f),
                midPoint + (Vector3.up * height),
                end + (Vector3.up * height * 0.5f),
                end
            };
        }

        public void AnimateCardSelect(Transform cardTransform, bool selected)
        {
            if (cardTransform == null) return;

            float targetY = selected ? 0.5f : 0f;
            Vector3 currentPos = cardTransform.localPosition;
            cardTransform.DOLocalMoveY(currentPos.y + targetY, 0.3f)
                .SetEase(Ease.OutQuad);
        }
    }
}