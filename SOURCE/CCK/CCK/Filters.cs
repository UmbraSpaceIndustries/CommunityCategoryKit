using UnityEngine;

namespace CCK
{
    /// <summary>
    /// Creates common categories. Running at <see cref="KSPAddon.Startup.Instantly"/> ensures CCK
    /// categories will always be first in the queue. 
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true /* once */)]
    public class CCK_CommonFilters : MonoBehaviour
    {
        void Awake() {
            FilterManager.AddFilter(
                "cck-containers", "Containers", "CommunityCategoryKit/Containers");
            FilterManager.AddFilter(
                "cck-lifesupport", "Life Support", "CommunityCategoryKit/LifeSupport");
        }
    }
}
