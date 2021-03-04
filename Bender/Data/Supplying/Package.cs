namespace Bender.Data.Supplying
{
    /// <summary>
    /// Prepared data for Bender's reaction: either to notify via email or to do REST request itself or so.
    /// </summary>
    /// <typeparam name="TItemType">Items: either Build or Jira Issue</typeparam>
    /// <typeparam name="TBendersReaction">Bender's reaction: send notification or update the issue itself</typeparam>
    internal class Package<TBendersReaction, TItemType> : PackageBase where TBendersReaction: new()
    {
        public TBendersReaction Reaction { get; set; } = new TBendersReaction();
        public TItemType[] Items { get; set; } = new TItemType[]{};
    }
}