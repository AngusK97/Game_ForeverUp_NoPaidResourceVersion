using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Misc
{
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float moveDelay;
        [SerializeField] private float moveDuration;
        
        private void Start()
        {
            StartCoroutine(MoveToTarget());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator MoveToTarget()
        {
            yield return new WaitForSeconds(moveDelay);
            var targetPos = target.position;
            var targetRot = target.rotation;
            transform.DOMove(targetPos, moveDuration);
            transform.DORotateQuaternion(targetRot, moveDuration);
        }
    }
}