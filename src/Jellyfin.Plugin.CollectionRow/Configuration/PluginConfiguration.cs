using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.CollectionRow.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Ids (Guid "N" or "D" format strings) of collections whose section should not be shown.
        /// </summary>
        public string[] DisabledCollectionIds { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Where to insert the section on the detail page: "aboveCast", "belowCast" or "top".
        /// </summary>
        public string SectionPosition { get; set; } = "aboveCast";

        /// <summary>
        /// How to mark the currently viewed item in the row: "ring", "dim" or "none".
        /// </summary>
        public string HighlightStyle { get; set; } = "ring";

        /// <summary>
        /// Whether the section is also shown on series detail pages.
        /// </summary>
        public bool IncludeSeries { get; set; }
    }
}
