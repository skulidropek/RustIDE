using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RustIDE.Server.Models.Dto
{
    public class MessageDto
    {
        [Key]
        public Guid Id { get; set; }

        public string Role { get; set; }
        public string Text { get; set; }

        [ForeignKey("ChatDto")]
        public Guid ChatDtoId { get; set; }

        public virtual ChatDto ChatDto { get; set; }
    }
}
