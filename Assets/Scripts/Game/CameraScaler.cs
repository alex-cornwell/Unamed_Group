using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    [SerializeField] private float targetAspect        = 16f / 9f;
    [SerializeField] private float baseOrthographicSize = 5f;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCameraSize();
    }

    private void UpdateCameraSize()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = baseOrthographicSize * (targetAspect / currentAspect);
    }
}
