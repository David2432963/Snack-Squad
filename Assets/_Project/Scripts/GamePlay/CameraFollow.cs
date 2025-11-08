using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing = 5f;
    public Vector3 offset;

    private void Start()
    {
        if (target == null) target = GameObject.Find("Player").transform;
    }

    void Update()
    {
        Vector3 targetCamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    }

    private void OnValidate()
    {
        transform.position = target.position + offset;
    }
}