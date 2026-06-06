using AppCoel.Models;
using System.ComponentModel.DataAnnotations;

namespace AppCoel.Core.Models.General
{
    public class SampleDto : IEntity
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;
        public byte[]? RowVersion { get; set; }
    }
}
