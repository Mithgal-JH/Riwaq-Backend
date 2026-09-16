namespace Team3.Backend.Models;

public class User
{
    public Guid Id { get; set; }

    public string FirebaseUid { get; set; } = string.Empty;

    // Learning Direction is optional when the user is created.
    // The user can select it later from the profile settings.
    public Guid? LearningDirectionId { get; set; }

    public LearningDirection? LearningDirection { get; set; }

    public Profile? Profile { get; set; }

    public int Points { get; set; }

    public ICollection<SkillVerificationRequest> RequestedSkillVerifications { get; set; } =
        new List<SkillVerificationRequest>();

    public ICollection<SkillVerificationRequest> MentoredSkillVerifications { get; set; } =
        new List<SkillVerificationRequest>();

    public ICollection<PointsTransaction> PointsTransactions { get; set; } =
        new List<PointsTransaction>();

    public ICollection<Progress> Progresses { get; set; } = new List<Progress>();

    public ICollection<EducationalContent> EducationalContents { get; set; } =
        new List<EducationalContent>();

    public ICollection<Like> Likes { get; set; } = new List<Like>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Share> Shares { get; set; } = new List<Share>();

    public ICollection<Save> Saves { get; set; } = new List<Save>();

    public ICollection<Repost> Reposts { get; set; } = new List<Repost>();

    public ICollection<Connection> ConnectionsAsUserA { get; set; } =
        new List<Connection>();

    public ICollection<Connection> ConnectionsAsUserB { get; set; } =
        new List<Connection>();

    public ICollection<ConnectionRequest> SentConnectionRequests { get; set; } =
        new List<ConnectionRequest>();

    public ICollection<ConnectionRequest> ReceivedConnectionRequests { get; set; } =
        new List<ConnectionRequest>();

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();

    public ICollection<Rating> ReceivedRatings { get; set; } = new List<Rating>();

    public ICollection<Notification> Notifications { get; set; } =
        new List<Notification>();

    public ICollection<UserSkill> UserSkills { get; set; } =
        new List<UserSkill>();

    public ICollection<UserInterest> UserInterests { get; set; } =
        new List<UserInterest>();
}
