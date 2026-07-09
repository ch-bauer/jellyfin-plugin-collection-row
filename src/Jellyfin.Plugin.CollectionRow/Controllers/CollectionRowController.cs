using Jellyfin.Plugin.CollectionRow.Configuration;
using Jellyfin.Plugin.CollectionRow.Model;
using Jellyfin.Plugin.CollectionRow.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.CollectionRow.Controllers
{
    [ApiController]
    [Route("CollectionRow")]
    public class CollectionRowController : ControllerBase
    {
        private readonly ILibraryManager m_libraryManager;
        private readonly CollectionLookupService m_lookupService;

        public CollectionRowController(ILibraryManager libraryManager, CollectionLookupService lookupService)
        {
            m_libraryManager = libraryManager;
            m_lookupService = lookupService;
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

            BaseItem? item = m_libraryManager.GetItemById(itemId);
            bool supported = item is Movie || (config.IncludeSeries && item is Series);
            if (!supported)
            {
                return Ok(new CollectionsResponse(
                    config.SectionPosition, config.HighlightStyle, Array.Empty<CollectionInfo>()));
            }

            HashSet<Guid> disabled = config.DisabledCollectionIds
                .Select(s => Guid.TryParse(s, out Guid g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToHashSet();

            IReadOnlyList<CollectionInfo> collections = m_lookupService.GetCollectionsForItem(itemId, disabled);

            return Ok(new CollectionsResponse(config.SectionPosition, config.HighlightStyle, collections));
        }
    }
}
