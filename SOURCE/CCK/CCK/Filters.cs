using KSP.UI.Screens;
using RUI.Icons.Selectable;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;



namespace CCK
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class CCK_ContainerFilter : BaseFilter
    {
        protected override string categoryTag
        {
            get { return "CCK_Containers"; }
            set { }
        }
        protected override string categoryTitle
        {
            get { return "Containers"; }
            set { }
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class CCK_LifeSupportFilter : BaseFilter
    {
        protected override string categoryTag
        {
            get { return "cck-lifesupport"; }  //Convention is to preface tag with cck
            set { }
        }
        protected override string categoryTitle
        {
            get { return "LifeSupport"; }
            set { }
        }
    }

    public abstract class BaseFilter : MonoBehaviour
    {
        private readonly List<AvailablePart> parts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal bool filter = true;
        protected abstract string categoryTag { get; set; }
        protected abstract string categoryTitle { get; set; }

        void Awake()
        {
            parts.Clear();
            foreach (var avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.tags.Contains(categoryTag))
                {
                    parts.Add(avPart);
                }
            }

            print(categoryTitle + "  Filter Count: " + parts.Count);
            if (parts.Count > 0)
                GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
        }

        private bool EditorItemsFilter(AvailablePart avPart)
        {
            return parts.Contains(avPart);
        }

        private void SubCategories()
        {
            var icon = GenIcon(categoryTitle);
            var filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(filter, categoryTitle, icon, EditorItemsFilter);
        }

        private Icon GenIcon(string iconName)
        {
            var normIcon = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var normIconFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), iconName + "_N.png");
            normIcon.LoadImage(File.ReadAllBytes(normIconFile));

            var selIcon = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var selIconFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), iconName + "_S.png");
            selIcon.LoadImage(File.ReadAllBytes(selIconFile));

            print("*****Adding icon for " + categoryTitle);
            var icon = new Icon(iconName + "Icon", normIcon, selIcon);
            return icon;
        }
    }

}