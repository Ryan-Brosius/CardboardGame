using DG.Tweening;
using UnityEngine;

public class Anim_Floating : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float moveStrength = 0.2f;
    [SerializeField] private float rotateStrength = 5f;
    [SerializeField] private int vibrato = 1;
    [SerializeField] private float randomness = 90f;

    private Tween moveTween;
    private Tween rotateTween;

    private void Start()
    {
        moveTween = transform.DOShakePosition(duration, moveStrength, vibrato, randomness, false, true, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);

        rotateTween = transform.DOShakeRotation(duration, rotateStrength, vibrato, randomness, true, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        moveTween?.Kill();
    }
}

