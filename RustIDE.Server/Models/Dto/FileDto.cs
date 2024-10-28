using System.ComponentModel.DataAnnotations;

namespace RustIDE.Server.Models.Dto
{
    public class FileDto
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Text { get; set; }
    }
}
