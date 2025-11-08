using OSK;
using UnityEngine;

public class DespawnOnDisable : MonoBehaviour
{
    private void OnDisable()
    {
        Main.Pool.Despawn(this);
    }
}
