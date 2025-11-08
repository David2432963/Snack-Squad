using System.Collections;
using UnityEngine;

public class CameraVibrate : MonoBehaviour
{
    [SerializeField] private Transform cam = null;
    [SerializeField] private float A = 1f;
    private float intensity;
    private bool isVibrating = false;
    private Vector3 originalPos;

    static CameraVibrate Instance;

    public static void Vibrate(float time, float intensity)
    {
        if (Instance.isVibrating) return;

        Instance.intensity = intensity;
        Instance.StopAllCoroutines();
        Instance.StartCoroutine(Instance.IEVibrate(time));
    }

    private IEnumerator IEVibrate(float time)
    {
        isVibrating = true;

        var A = this.A;
        var t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            var y = Mathf.Sin(t * intensity * 10f) * A;
            var x = Mathf.Sin(t * intensity * 10f) * A;
            cam.localPosition = new Vector3(x, y, cam.localPosition.z);
            A -= this.A * Time.deltaTime / time;
            yield return null;
        }

        cam.localPosition = originalPos;
        isVibrating = false;
    }


    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        originalPos = cam.localPosition;
    }
}
