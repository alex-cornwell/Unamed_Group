using UnityEngine;

public class ImageShake : MonoBehaviour
{
    [SerializeField] private float shakeAmount = 2f;
    [SerializeField] private float shakeSpeed  = 6f;

    private Vector3 startPos;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float x = Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) * 2f - 1f;
        float y = Mathf.PerlinNoise(0f, Time.time * shakeSpeed) * 2f - 1f;
        rectTransform.anchoredPosition = startPos + new Vector3(x, y, 0f) * shakeAmount;
    }
}
