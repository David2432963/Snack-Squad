using System;
using OSK;
using UnityEngine;

public class Food : MonoBehaviour
{
    public Action OnRemoved;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnRemoved?.Invoke();
            Main.Pool.Despawn(this);
            Particle_Manager.Instance.PlayRandomBlowFx(transform.position);
        }
    }
}
