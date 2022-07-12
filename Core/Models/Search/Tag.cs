namespace Core.Models.Search
{
    public class Tag
    {
        public string TagNo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string McPkgNo { get; set; } = string.Empty;
        public string CommPkgNo { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string DisciplineCode { get; set; } = string.Empty;
        public string DisciplineDescription { get; set; } = string.Empty;
        public string CallOffNo { get; set; } = string.Empty;
        public string PurchaseOrderNo { get; set; } = string.Empty;
        public string TagFunctionCode { get; set; } = string.Empty;
        public const string TopicName = "tag";
    }
}