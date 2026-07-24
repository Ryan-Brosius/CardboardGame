using DG.Tweening;
using UnityEngine;

public class RotateTween : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float rotateStrength = 5f;
    [SerializeField] private int vibrato = 1;
    [SerializeField] private float randomness = 90f;

    private Tween rotateTween;

    private void Start()
    {
        rotateTween = transform.DOShakeRotation(duration, rotateStrength, vibrato, randomness, true, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        rotateTween?.Kill();
    }
}
