namespace Core.Models.Search
{
    public class IndexDocument
    {
        public string Key { get; set; } = "";
        public DateTime LastUpdated { get; set; }
        public string Plant { get; set; } = "";
        public string PlantName { get; set; } = "";
        public string Project { get; set; } = "";
        public List<string> ProjectNames { get; set; } = new List<string>();
        public CommPkg? CommPkg { get; set; }
        public McPkg? McPkg { get; set; }
        public Tag? Tag { get; set; }
        public PunchItem? PunchItem { get; set; }
    }
}
