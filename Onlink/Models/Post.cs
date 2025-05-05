using System.ComponentModel.DataAnnotations;

namespace Onlink.Models
{
    public class Post
    {
        public int PostId { get; set; }

        // Foreign keys
        public int EmployeeId { get; set; }
        public int EmployerId { get; set; }

        [Required]
        public ActivityType ActivityType { get; set; }
        public string MediaUrl { get; set; }
        public MediaType MediaType { get; set; }
        public PostPrivacy Privacy { get; set; } = PostPrivacy.Public;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public int ShareCount { get; set; } = 0;

        // Navigation properties
        public Employee Employee { get; set; }
        public Employer Employer { get; set; }

        // Corrected self-referential relationship
        public ICollection<Post> RelatedPosts { get; set; } = new List<Post>();
        public int? ParentPostId { get; set; }  // Nullable foreign key for self-reference
        public Post? ParentPost { get; set; }
    }

    public enum ActivityType
    {
        Post,
        Like,
        Comment,
        Share,
        Save
    }

    public enum MediaType
    {
        None,
        Image,
        Video
    }

    public enum PostPrivacy
    {
        Public,
        FriendsOnly,
        Private
    }
}
