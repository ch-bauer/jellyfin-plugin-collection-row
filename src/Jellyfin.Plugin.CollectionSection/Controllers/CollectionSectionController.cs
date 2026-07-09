using Jellyfin.Data.Enums;
using Jellyfin.Plugin.CollectionSection.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.CollectionSection.Controllers
{
    [ApiController]
    [Route("CollectionSection")]
    public class CollectionSectionController : ControllerBase
    {
        private readonly ILibraryManager m_libraryManager;

        public CollectionSectionController(ILibraryManager libraryManager)
        {
            m_libraryManager = libraryManager;
        }

        /// <summary>
        /// Returns the collections (BoxSets) that contain the given item plus the
        /// client-relevant plugin settings, excluding collections disabled in the
        /// plugin configuration.
        /// </summary>
        [HttpGet("Collections")]
        [Authorize]
        public ActionResult<CollectionsResponse> GetCollections([FromQuery] Guid itemId)
        {
            if (itemId == Guid.Empty)
            {
                return BadRequest("itemId is required.");
            }

            PluginConfiguration config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
            CollectionsResponse emptyResponse = new CollectionsResponse(
                config.SectionPosition, config.HighlightStyle, Array.Empty<CollectionInfo>());

            BaseItem? item = m_libraryManager.GetItemById(itemId);
            bool supported = item is Movie || (config.IncludeSeries && item is Series);
            if (!supported)
            {
                return Ok(emptyResponse);
            }

            HashSet<Guid> disabled = config.DisabledCollectionIds
                .Select(s => Guid.TryParse(s, out Guid g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToHashSet();

            List<CollectionInfo> collections = m_libraryManager.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { BaseItemKind.BoxSet },
                    CollapseBoxSetItems = false,
                    Recursive = true
                })
                .OfType<BoxSet>()
                .Where(boxSet => !disabled.Contains(boxSet.Id))
                .Where(boxSet => boxSet.GetLinkedChildren().Any(child => child.Id == itemId))
                .OrderBy(boxSet => boxSet.SortName, StringComparer.OrdinalIgnoreCase)
                .Select(boxSet => new CollectionInfo(boxSet.Id, boxSet.Name))
                .ToList();

            return Ok(new CollectionsResponse(config.SectionPosition, config.HighlightStyle, collections));
        }
    }

    public record CollectionInfo(Guid Id, string Name);

    public record CollectionsResponse(string SectionPosition, string HighlightStyle, IReadOnlyList<CollectionInfo> Collections);
}
