using System.ComponentModel.DataAnnotations;

namespace Onlink.Models
{
    public class Post
    {
        public int PostId { get; set; }
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


        public IEnumerable<Post> RelatedActivities { get; set; } = new List<Post>();
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
