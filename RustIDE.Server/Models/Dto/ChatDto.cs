using System.ComponentModel.DataAnnotations;

namespace RustIDE.Server.Models.Dto
{
    public class ChatDto
    {
        [Key]
        public Guid Id { get; set; }
        public virtual List<MessageDto> Messages { get; set; }
    }
}
