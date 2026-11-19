using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreUIPunch : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float duration = 0.4f;

    [Header("Scale Pop")]
    [SerializeField] private float punchScale = 0.3f;
    [SerializeField] private float scaleWobbles = 2.5f;

    [Header("Rotation Wobble")]
    [SerializeField] private float maxAngle = 8f;
    [SerializeField] private float rotationWobbles = 3.5f;

    [Header("Position Shake")]
    [SerializeField] private float shakeDistance = 6f;
    [SerializeField] private float shakeSpeed = 35f;

    [Header("Color Flash")]
    [SerializeField] private Color flashColor = new Color(1f, 0.95f, 0.35f);
    [SerializeField] private float flashTime = 0.15f;

    private TextMeshProUGUI text;
    private RectTransform rect;

    private Vector3 baseScale;
    private Vector2 basePosition;
    private Quaternion baseRotation;
    private Color baseColor;

    private Coroutine running;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        rect = text.rectTransform;
        CacheBase();
    }

    private void OnDisable()
    {
        if (running != null) StopCoroutine(running);
        running = null;
        ResetToBase();
    }

    public void Punch()
    {
        if (running != null) StopCoroutine(running);
        ResetToBase();
        running = StartCoroutine(PunchRoutine(text));
    }

    private IEnumerator PunchRoutine(TextMeshProUGUI target)
    {
        RectTransform rt = target.rectTransform;
        float seedX = Random.value * 100f;
        float seedY = Random.value * 100f;
        float rotDir = Random.value < 0.5f ? -1f : 1f;

        float t = 0f;
        while (t < duration)
        {
            float n = t / duration;
            float decay = (1f - n) * (1f - n);

            float scaleOffset = Mathf.Cos(n * scaleWobbles * Mathf.PI * 2f) * decay * punchScale;
            rt.localScale = baseScale * (1f + scaleOffset);

            float angle = Mathf.Sin(n * rotationWobbles * Mathf.PI * 2f) * decay * maxAngle * rotDir;
            rt.localRotation = baseRotation * Quaternion.Euler(0f, 0f, angle);

            float shakeDecay = decay * (1f - n);
            float sx = (Mathf.PerlinNoise(seedX, t * shakeSpeed) - 0.5f) * 2f;
            float sy = (Mathf.PerlinNoise(seedY, t * shakeSpeed) - 0.5f) * 2f;
            rt.anchoredPosition = basePosition + new Vector2(sx, sy) * shakeDistance * shakeDecay;

            target.color = t < flashTime ? Color.Lerp(flashColor, baseColor, t / flashTime) : baseColor;

            t += Time.deltaTime;
            yield return null;
        }

        ResetToBase();
        running = null;
    }

    private void CacheBase()
    {
        baseScale = rect.localScale;
        basePosition = rect.anchoredPosition;
        baseRotation = rect.localRotation;
        baseColor = text.color;
    }

    private void ResetToBase()
    {
        if (rect == null) return;
        rect.localScale = baseScale;
        rect.anchoredPosition = basePosition;
        rect.localRotation = baseRotation;
        text.color = baseColor;
    }

    public void RebaseRestPose()
    {
        if (running != null) StopCoroutine(running);
        running = null;
        ResetToBase();
        CacheBase();
    }
}
