using UnityEngine;

public class PuppetMovement : MonoBehaviour
{
    [Header("Vertical Bob")]
    [SerializeField] private float verticalAmount = 0.1f;
    [SerializeField] private float verticalSpeed = 0.8f;

    [Header("Horizontal Sway")]
    [SerializeField] private float horizontalAmount = 0.15f;
    [SerializeField] private float horizontalSpeed = 0.5f;

    [Header("Dangle")]
    [SerializeField] private float maxTiltAngle = 8f;
    [SerializeField] private float tiltSpeed = 1.2f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 noiseSeed;
    private bool movementEnabled;

    private void Awake()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        movementEnabled = true;

        noiseSeed = new Vector3(
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f)
        );
    }

    private void Update()
    {
        if (!movementEnabled) return;
        float time = Time.time;

        float rawX = Mathf.PerlinNoise(noiseSeed.x + time * horizontalSpeed, 0f) - 0.5f;
        float rawY = Mathf.PerlinNoise(0f, noiseSeed.y + time * verticalSpeed) - 0.5f;

        Vector3 positionOffset = new Vector3(
            rawX * 2f * horizontalAmount,
            rawY * 2f * verticalAmount,
            0f
        );

        transform.localPosition = startPosition + positionOffset;

        float rawTilt = Mathf.PerlinNoise(noiseSeed.z + time * tiltSpeed, 0f) - 0.5f;
        float currentTilt = rawTilt * 2f * maxTiltAngle;

        transform.localRotation = startRotation * Quaternion.Euler(0f, 0f, currentTilt);
    }

    public void PauseMovement()
    {
        movementEnabled = false;
    }

    public void StartMovement()
    {
        movementEnabled = true;
    }
}
