
namespace SPPersonalViewMigrate
{
    public class View
    {
        public string WebUrl { get; set; }
        public string ListUrl { get; set; }
        public string UserLogin { get; set; }
        public string ViewName { get; set; }
        public string ContentTypeId { get; set; }
        public long Flags { get; set; }
        public string ViewSchema { get; set; }

        public override string ToString()
        {
            return string.Format("WebUrl={0}   ListUrl={1}   UserLogin={2}   ViewName={3}   ContentTypeId={4}   Flags={5}   ViewSchema={6}", WebUrl, ListUrl, UserLogin, ViewName, ContentTypeId, Flags, ViewSchema);
        }
    }
}
