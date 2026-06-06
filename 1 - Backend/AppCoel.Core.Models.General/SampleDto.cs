using AppCoel.Models;
using AppCoel.Models.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace AppCoel.Core.Models.General
{
    public class SampleDto : IEntity
    {
        [RequiredGuid]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;
        public byte[]? RowVersion { get; set; }
    }
}
