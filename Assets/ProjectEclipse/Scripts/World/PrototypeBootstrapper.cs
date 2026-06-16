using UnityEngine;

namespace ProjectEclipse.World
{
    public class PrototypeBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            Debug.LogWarning("PrototypeBootstrapper is deprecated. ProjectEclipse_MVP now uses serialized scene objects and MvpGameManager.");
        }
    }
}
