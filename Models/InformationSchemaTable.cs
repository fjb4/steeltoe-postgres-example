namespace PostgresExample.Models
{
    public class InformationSchemaTable
    {
        public string table_catalog { get; set; }
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string table_type { get; set; }
    }
}