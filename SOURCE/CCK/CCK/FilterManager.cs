// License: Attribution-NonCommercial-ShareAlike 4.0 International

using KSP.UI.Screens;
using RUI.Icons.Selectable;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CCK
{
    class PartsFilter
    {
        /// <summary>Tag for the category filter.</summary>
        public string categoryTag { get; private set; }

        readonly string categoryTitle;
        readonly List<AvailablePart> avParts = new List<AvailablePart>();
        readonly string categoryIconTextureUrlPrefix;
        const string Category = "Filter by Function";

        /// <summary>Creates a filter.</summary>
        /// <param name="tag">Tag to filter parts for.</param>
        /// <param name="title">Title (hint) of the category icon in the editor.</param>
        /// <param name="iconTextureUrlPrefix">
        /// Game database URL prefix to search for the icon textures.
        /// </param>
        public PartsFilter(string tag, string title, string iconTextureUrlPrefix)
        {
            categoryTag = tag;
            categoryTitle = title;
            categoryIconTextureUrlPrefix = iconTextureUrlPrefix;
            Debug.LogFormat("New CCK filter \"{0}\": tag={1}, icon={2}",
                            categoryTitle, categoryTag, categoryIconTextureUrlPrefix);
        }

        /// <summary>
        /// Verifies if part macthes the filetr, and adds it into the part's list.
        /// </summary>
        /// <param name="avPart">Part to check.</param>
        public void CheckPart(AvailablePart avPart)
        {
            if (avPart.tags.Contains(categoryTag))
            {
                avParts.Add(avPart);
            }
        }

        /// <summary>
        /// Creates new category in the game. DOes nothing if no parts were found for the filter.
        /// </summary>
        public void AddCategory()
        {
            if (avParts.Count > 0)
            {
                Debug.LogFormat("CCK category \"{0}\" part(s) count: {1}",
                                categoryTitle, avParts.Count);
                GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
            }
            else
            {
                Debug.LogFormat("Skip CCK category \"{0}\" since there is no parts in it",
                                categoryTitle);
            }
        }

        /// <summary>
        /// Callback to be called by the KSP editor when it's time to populate the filter.
        /// </summary>
        void SubCategories()
        {
            var filter = PartCategorizer.Instance.filters.Find(
                f => f.button.categoryName == Category);
            PartCategorizer.AddCustomSubcategoryFilter(
                filter, categoryTitle, GenIcon(), avParts.Contains);
        }

        /// <summary>Creates an icon from the category textures.</summary>
        /// <returns>Icon with normal and selected state.</returns>
        Icon GenIcon()
        {
            // Get normal and selected textures, and create the icon.
            var selectedIcon = GameDatabase.Instance.GetTexture(
                categoryIconTextureUrlPrefix + "_S", false /* asNormalMap */);
            var normalIcon = GameDatabase.Instance.GetTexture(
                categoryIconTextureUrlPrefix + "_N", false /* asNormalMap */);
            var icon = new Icon(categoryIconTextureUrlPrefix + "Icon", normalIcon, selectedIcon);
            return icon;
        }
    }

    /// <summary>
    /// Main entry for the CCK functionality. Use this class to create custom filters.
    /// </summary>
    /// <example>
    /// <para>
    /// Create an object that runs once on the game start to create new category. CCK will handle
    /// this information to have actual stuff done in the KSP editor.
    /// </para>
    /// <code><![CDATA[
    /// [KSPAddon(KSPAddon.Startup.MainMenu, true /* once */)]
    /// public class MyNewFilter : MonoBehaviour
    /// {
    ///    void Awake() {
    ///        FilterManager.AddFilter("cck-cool-stuff-tag", "Cool Stuff",
    ///                                "CoolStuff/Images/FilterIcon");
    ///    }
    /// }
    /// ]]></code>
    /// <para>
    /// When game loads all the parts that have <c>"cck-cool-stuff-tag"</c> tag will be grouped in
    /// the editor under filter named "Cool Stuff". The icon for the filter will be created from
    /// two textures:
    /// </para>
    /// <list type="bullet">
    /// <item><c>"CoolStuff/Images/FilterIcon_N"</c> for the normal mode.</item>
    /// <item><c>"CoolStuff/Images/FilterIcon_S"</c> for the selected mode.</item>
    /// </list>
    /// <para>
    /// Textures can be of any type which is recognized by Unity (e.g. <c>PNG</c>, <c>DDS</c>,
    /// etc.).
    /// </para>
    /// </example>
    [KSPAddon(KSPAddon.Startup.MainMenu, true /* once */)]
    public class FilterManager : MonoBehaviour
    {
        /// <summary>All filters managed by CCK.</summary>
        readonly static List<PartsFilter> filters = new List<PartsFilter>();

        /// <summary>Called by Unity once the scene starts.</summary>
        void Awake()
        {
            // Delay execution to let othe mods adding their categories. 
            StartCoroutine(WaitAndCreateFilters());
        }

        /// <summary>
        /// Adds filter for a tag if one doesn't exist. In case of there is a filter with the same
        /// tag no duplicate will be created.
        /// </summary>
        /// <param name="tag">
        /// Tag to check in the parts. Good style is to have it prefixed with <c>"cck-"</c> string
        /// to indicate it's a CCK tag.
        /// <para>
        /// The tag can be absolutely any string as long as your mod is the only one that uses it.
        /// If you're going to re-use a tag from some other mod do communicate to the author about
        /// your intention, and let CCK developers know there is another common category candidate.
        /// In general, it's enough to have at least two mods using the same tag to have new common
        /// category added into CCK.
        /// </para>
        /// </param>
        /// <param name="title">
        /// User friendly string for the name of the filter. Note that if there are several filters
        /// with the same tag then title from the first call will be used. It's undetermined which
        /// mod will be the first in the chain, so keep names consistent.
        /// </param>
        /// <param name="iconUrlPrefix">
        /// URL prefix to the icon's texture files. Two specific textures are expected for the
        /// prefix:
        /// <list type="bullet">
        /// <item><c>"&lt;prefix&gt;_N"</c> for a normal texture.</item>
        /// <item><c>"&lt;prefix&gt;_S"</c> for a selected texture.</item>
        /// </list>
        /// </param>
        public static void AddFilter(string tag, string title, string iconUrlPrefix)
        {
            if (filters.All(x => x.categoryTag != tag))
            {
                filters.Add(new PartsFilter(tag, title, iconUrlPrefix));
            }
        }

        /// <summary>Waits for the end of frame and populates categories to the editor.</summary>
        /// <remarks>
        /// Waiting is needed to resolve race condition with the mods that initialize at the
        /// <see cref="KSPAddon.Startup.MainMenu"/> scene.
        /// </remarks>
        IEnumerator WaitAndCreateFilters()
        {
            // Wait for the mods to load and request filters.
            yield return new WaitForEndOfFrame();

            // Pass parts thru the filters.
            PartLoader.LoadedPartsList
                .Where(avPart => avPart.partPrefab != null)
                .ToList()
                .ForEach(avPart => filters.ForEach(x => x.CheckPart(avPart)));

            // Add the filters into the game.
            filters.ForEach(x => x.AddCategory());
        }
    }
}
