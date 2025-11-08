using OSK;
using UnityEngine;

public class Particle_Manager : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] blowFxs;

    public static Particle_Manager Instance => SingletonManager.Instance.Get<Particle_Manager>();
    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }

    public void PlayRandomBlowFx(Vector3 position)
    {
        if (blowFxs.Length == 0) return;

        Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, blowFxs[Random.Range(0, blowFxs.Length)], null, position);
    }
}