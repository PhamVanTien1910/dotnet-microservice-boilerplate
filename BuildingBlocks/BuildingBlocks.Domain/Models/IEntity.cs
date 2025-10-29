using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Domain.Models
{
    public interface IEntity<out TKey>
    {
        TKey Id { get; }
    }
    public class Entity : IEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
