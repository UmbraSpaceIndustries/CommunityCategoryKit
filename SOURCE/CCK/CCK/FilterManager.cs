// License: Attribution-NonCommercial-ShareAlike 4.0 International

using KSP.UI.Screens;
using RUI.Icons.Selectable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CCK
{
    class PartsFilter
    {
        /// <summary>Tag for the category filter.</summary>
        public readonly string tag;
        /// <summary>Category ttitle to show in the editor.</summary>
        public readonly string title;
        /// <summary>Tells if filter is defined by the CCK core.</summary>
        public readonly bool isCommon;

        /// <summary>Texture URL the icon in normal state.</summary>
        readonly string normalTextureName;
        /// <summary>
        /// Texture URL the icon in selected state. If omitted or not found then a simple icon will
        /// be created with selected a texture automatically generated.
        /// </summary>
        readonly string selectedTextureName;
        /// <summary>All parts that matched the filter.</summary>
        readonly List<AvailablePart> avParts = new List<AvailablePart>();
        /// <summary>Editor button to attach filter to.</summary>
        const string Category = "Filter by function";

        /// <summary>Creates a filter.</summary>
        /// <param name="tag">
        /// Tag to check in the parts. Good style is to have it prefixed with <c>"cck-"</c> string
        /// to indicate it's a CCK tag.
        /// </param>
        /// <param name="title">Title (hint) of the category icon in the editor.</param>
        /// <param name="normalTextureName">
        /// Game database URL to a texture for icon normal state.
        /// </param>
        /// <param name="selectedTextureName">
        /// Game database URL to a texture for icon selected state. Can be empty, in which case
        /// texture for the selected state will be auto-generated.
        /// </param>
        /// <param name="isCommon">
        /// Tells if category is a CCK native category. Such categories cannot be overwritten by the
        /// third-party mods.
        /// </param>
        public PartsFilter(string tag, string title,
                           string normalTextureName, string selectedTextureName, bool isCommon)
        {
            this.tag = tag.ToLower();  // In KSP tags are case-insensitive.
            this.title = title;
            this.normalTextureName = normalTextureName;
            this.selectedTextureName = selectedTextureName;
            this.isCommon = isCommon;
        }

        /// <summary>
        /// Verifies if part matches the filter, and adds it into the part's list.
        /// </summary>
        /// <param name="avPart">Part to check.</param>
        public void CheckPart(AvailablePart avPart)
        {
            if (avPart.tags.Contains(tag))
            {
                avParts.Add(avPart);
            }
        }

        /// <summary>
        /// Creates new filter in the game. Does nothing if no parts were found for the filter.
        /// </summary>
        public void AddFilter()
        {
            if (avParts.Count > 0)
            {
                Debug.LogFormat("Add CCK filter \"{0}\" with {1} part(s)", title, avParts.Count);
                GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
            }
            else
            {
                Debug.LogFormat("Skip CCK filter \"{0}\" since there is no parts in it", title);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
          return string.Format(
              "title={0}, tag={1}, normIcon={2}, selIcon={3}, isCommon={4}",
              title, tag, normalTextureName, selectedTextureName, isCommon);
        }

        /// <summary>
        /// Callback to be called by the KSP editor when it's time to populate the filter.
        /// </summary>
        void SubCategories()
        {
            var filter = PartCategorizer.Instance.filters.Find(
                f => f.button.categoryName == Category);
            PartCategorizer.AddCustomSubcategoryFilter(
                filter, title, title, GenIcon(), avParts.Contains);
        }

        /// <summary>Creates an icon from the textures.</summary>
        /// <returns>Icon with normal and selected state.</returns>
        Icon GenIcon()
        {
            // Get normal and selected textures, and create the icon.
            var normalTexture = GameDatabase.Instance.GetTexture(
                normalTextureName, false /* asNormalMap */);
            var selectedTexture = GameDatabase.Instance.GetTexture(
                selectedTextureName, false /* asNormalMap */);
            return new Icon(tag + "-icon", normalTexture, selectedTexture ?? normalTexture,
                            simple: selectedTexture == null);
        }
    }

    /// <summary>
    /// Main entry for the CCK functionality. It loads configuration settings and creates the
    /// appropriate filters in the edtior. Custom mods can upgrade configuration via ModuleManager
    /// to construct own filters.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true /* once */)]
    class FilterManager : MonoBehaviour
    {
        /// <summary>All filters managed by CCK.</summary>
        static readonly List<PartsFilter> filters = new List<PartsFilter>();

        /// <summary>Called by Unity once the scene starts.</summary>
        void Awake() {
            // Collect filter definitions.
            var commonItemsNode = GameDatabase.Instance.GetConfigNode(
                "CommunityCategoryKit/common-filters/CCKCommonFilterConfig");
            if (commonItemsNode != null)
            {
                AddFiltersFromConfig(commonItemsNode, isCommonTag: true);
            }
            var extraItemsNode = GameDatabase.Instance.GetConfigNode(
                "CommunityCategoryKit/extra-filters/CCKExtraFilterConfig");
            if (extraItemsNode != null)
            {
                AddFiltersFromConfig(extraItemsNode, isCommonTag: false);
            }

            // Pass parts thru the filters.
            PartLoader.LoadedPartsList
                .Where(avPart => avPart.partPrefab != null)
                .ToList()
                .ForEach(avPart => filters.ForEach(x => x.CheckPart(avPart)));

            // Add the filters into the game.
            filters.ForEach(x => x.AddFilter());
        }

        /// <summary>
        /// Adds filter for a tag if one doesn't exist. In case of there is a filter with the same
        /// tag a better candidate will be chosen.
        /// </summary>
        /// <remarks>
        /// When a filter for an existing tag is being added a decision is made on which filter to
        /// keep:
        /// <list type="bullet">
        /// <item>Native (a.k.a. "common") CCK filter is always favored over a custom filter.</item>
        /// <item>
        /// Custom filters are compared by their title and the one which is lexicographically "less"
        /// (case-sensitive) will be kept, and the other one dropped.
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="newFilter">Filter description to add.</param>
        void AddFilter(PartsFilter newFilter)
        {
            var existingFilter = filters.FirstOrDefault(x => x.tag == newFilter.tag);
            if (existingFilter != null && newFilter.isCommon)
            {
                // Normally never happens. 
                Debug.LogErrorFormat("Duplicated common filter: {0}. Ignoring.", tag);
            }
            else if (existingFilter == null)
            {
                Debug.LogFormat("Create new CCK filter: {0}", newFilter);
                filters.Add(newFilter);
            }
            else if (existingFilter != null)
            {
                // A very simple tie-break approach. Main idea here is being consistent, i.e. always
                // showing the same category on game load.
                var tieBreakValue = string.Compare(
                    newFilter.title, existingFilter.title, StringComparison.Ordinal);
                if (!existingFilter.isCommon && tieBreakValue < 0)
                {
                    Debug.LogWarningFormat(
                        "Replacing existing CCK filter with a new one: existing=[{0}], new=[{1}]",
                        existingFilter, newFilter);
                    filters.Remove(existingFilter);
                    filters.Add(newFilter);
                }
                else
                {
                    Debug.LogWarningFormat("Ignoring new CCK filter in favor of the existing one:"
                                           + " existing=[{0}], new=[{1}]",
                                           existingFilter, newFilter);
                }
            }
        }

        /// <summary>
        /// Reads filter definitions from a node and adds them into the manager.
        /// </summary>
        /// <param name="node">Node to get data from.</param>
        /// <param name="isCommonTag">Specifies if node describes CCK native categories.</param>
        void AddFiltersFromConfig(ConfigNode node, bool isCommonTag)
        {
            foreach (var item in node.GetNodes("Item"))
            {
                var filterItem = new PartsFilter(
                    item.GetValue("tag"), item.GetValue("name"),
                    item.GetValue("normalIcon"), item.GetValue("selectedIcon"),
                    isCommonTag);
                AddFilter(filterItem);
            }
        }
    }
}
