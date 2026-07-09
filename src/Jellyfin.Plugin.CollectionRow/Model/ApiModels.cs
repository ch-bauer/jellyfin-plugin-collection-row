namespace Jellyfin.Plugin.CollectionRow.Model
{
    public record CollectionInfo(Guid Id, string Name);

    public record CollectionsResponse(string SectionPosition, string HighlightStyle, IReadOnlyList<CollectionInfo> Collections);
}
