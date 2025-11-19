using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Core
{
    public class IpadChecker : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private bool isIpad = false;
#endif
        
        public bool IsIPad()
        {
#if UNITY_EDITOR
            return isIpad;
#elif UNITY_IOS
            var gen = Device.generation;
            if (gen == DeviceGeneration.iPadUnknown)
                return true;                    // iPad, модель не распознана

            var name = gen.ToString();
            return name.StartsWith("iPad", System.StringComparison.Ordinal);
#else
        return false;
#endif
        }
    }
}