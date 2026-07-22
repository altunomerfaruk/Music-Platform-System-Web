
namespace MusicProject.Models.Core
{
    public class BaseEntities
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
