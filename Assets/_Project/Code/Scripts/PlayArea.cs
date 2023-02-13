using UnityEngine;

namespace _Project.Code.Scripts
{
    public class PlayArea : MonoBehaviour
    {
        [SerializeField] private Transform _onDeckTransform;
        [SerializeField] private Transform _bubbleSlotTransform;

        public static float BubbleScale = 0.5f;

        // Start is called before the first frame update
        void Start()
        {
            _onDeckTransform.localScale = new Vector3(BubbleScale, BubbleScale, BubbleScale);
            _bubbleSlotTransform.localScale = new Vector3(BubbleScale, BubbleScale, BubbleScale);
        }
    }
}
