namespace Core.Models.DbStatus
{
    public class MetricDto
    {
        public string UserName { get; set; } = "";
        public string Program { get; set; } = "";
        public int Sid { get; set; }
        public int Serial { get; set; }
        public string Name { get; set; } = ""; 
        public long Value { get; set; }
    }
}
