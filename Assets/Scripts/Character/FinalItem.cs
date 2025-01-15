using DG.Tweening;
using UnityEngine;

namespace Character
{
    public class FinalItem : MonoBehaviour
    {
        [SerializeField] private float posYOffset = 3f;
        [SerializeField] private float moveDuration = 0.2f;
        [SerializeField] private Ease ease;
        
        public void Move(Vector3 targetPos)
        {
            var endPos = new Vector3(targetPos.x, targetPos.y + posYOffset, targetPos.z);
            transform.DOMove(endPos, moveDuration).SetEase(ease);
        }

        public void SetPos(Vector3 position)
        {
            var endPos = new Vector3(position.x, position.y + posYOffset, position.z);
            transform.position = endPos;
        }
    }
}