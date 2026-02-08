using System.ComponentModel.DataAnnotations;

namespace IdempotenceGuide.Entity
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        [Timestamp] // EF Core concurrency token
        public byte[] RowVersion { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
