using DG.Tweening;
using UnityEngine;

public class ShakeTween : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float moveStrength = 0.2f;
    [SerializeField] private int vibrato = 1;
    [SerializeField] private float randomness = 90f;

    private Tween shakeTween;

    private void Start()
    {
        shakeTween = transform.DOShakePosition(duration, moveStrength, vibrato, randomness, false, true, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        shakeTween?.Kill();
    }
}
