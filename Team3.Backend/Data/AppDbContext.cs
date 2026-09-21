using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Team3.Backend.Models;

namespace Team3.Backend.Data;

public class AppDbContext
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<LearningDirection> LearningDirections => Set<LearningDirection>();
    public DbSet<Progress> Progresses => Set<Progress>();
    public DbSet<EducationalContent> EducationalContents => Set<EducationalContent>();
    public DbSet<ContentAnalysis> ContentAnalyses => Set<ContentAnalysis>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Share> Shares => Set<Share>();
    public DbSet<Save> Saves => Set<Save>();
    public DbSet<Repost> Reposts => Set<Repost>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<ConnectionRequest> ConnectionRequests => Set<ConnectionRequest>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<LearningSession> LearningSessions => Set<LearningSession>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Interest> Interests => Set<Interest>();

    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<UserInterest> UserInterests => Set<UserInterest>();
    public DbSet<LearningDirectionSkill> LearningDirectionSkills =>
        Set<LearningDirectionSkill>();
    public DbSet<LearningDirectionInterest> LearningDirectionInterests =>
        Set<LearningDirectionInterest>();
    public DbSet<EducationalContentSkill> EducationalContentSkills =>
        Set<EducationalContentSkill>();
    public DbSet<EducationalContentInterest> EducationalContentInterests =>
        Set<EducationalContentInterest>();
    public DbSet<EducationalContentLearningDirection>
        EducationalContentLearningDirections =>
        Set<EducationalContentLearningDirection>();

    public DbSet<SkillVerificationRequest> SkillVerificationRequests =>
    Set<SkillVerificationRequest>();

    public DbSet<PointsTransaction> PointsTransactions =>
        Set<PointsTransaction>();

    public DbSet<PointsPurchase> PointsPurchases => Set<PointsPurchase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirebaseUid)
                .IsRequired();

            entity.Property(x => x.Points)
                .HasDefaultValue(50);

            entity.HasIndex(x => x.FirebaseUid)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            // Learning Direction is optional for a User.
            // A user can be created without selecting a learning direction.
            entity.HasOne(x => x.SelectedSkill)
                .WithMany()
                .HasForeignKey(x => x.LearningDirectionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PointsTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Reason)
                .IsRequired();

            entity.Property(x => x.TransactionType)
                .HasConversion<string>()
                .HasDefaultValue(PointsTransactionType.Legacy)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.UserId,
                x.TransactionType
            })
            .IsUnique()
            .HasFilter("\"TransactionType\" = 'InitialBalance'");

            entity.HasIndex(x => x.MentoringSessionId)
                .IsUnique()
                .HasFilter("\"MentoringSessionId\" IS NOT NULL");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.RelatedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PointsPurchase>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PackageId).IsRequired();
            entity.Property(x => x.IdempotencyKey).IsRequired();
            entity.Property(x => x.Currency).IsRequired();
            entity.Property(x => x.PaymentMethod).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.Reference).IsRequired();
            entity.Property(x => x.Price).HasPrecision(18, 2);

            entity.HasIndex(x => new { x.UserId, x.IdempotencyKey })
                .IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.UserId)
                .IsUnique();

            entity.HasOne(x => x.User)
                .WithOne(x => x.Profile)
                .HasForeignKey<Profile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningDirection>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.Ignore(x => x.Users);
        });

        modelBuilder.Entity<Progress>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Progresses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.LearningDirection)
                .WithMany(x => x.Progresses)
                .HasForeignKey(x => x.LearningDirectionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.LearningDirectionId
            }).IsUnique();
        });

        modelBuilder.Entity<EducationalContent>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ContentVersion)
                .HasDefaultValue(1)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.EducationalContents)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContentAnalysis>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestId)
                .IsRequired();

            entity.Property(x => x.AnalysisState)
                .IsRequired();

            entity.Property(x => x.PrimaryTopicsJson)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(x => x.SecondaryTopicsJson)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(x => x.RiskCategoriesJson)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(x => x.DifficultyConfidence)
                .HasPrecision(18, 8);

            entity.Property(x => x.SafetyConfidence)
                .HasPrecision(18, 8);

            entity.Property(x => x.SafetyThreshold)
                .HasPrecision(18, 8);

            entity.Property(x => x.SafetyReviewRequired)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.EducationalContentId,
                x.ContentVersion
            })
            .IsUnique();

            entity.HasIndex(x => x.RequestId)
                .IsUnique();

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.ContentAnalyses)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.EducationalContentId
            }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ParentComment)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Share>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Shares)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.Shares)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Save>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.EducationalContentId
            }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Saves)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.Saves)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Repost>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.EducationalContentId
            }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Reposts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.Reposts)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Connection>(entity =>
   {
       entity.HasKey(x => x.Id);

       entity.HasIndex(x => new { x.UserAId, x.UserBId })
           .IsUnique();

       entity.HasCheckConstraint(
           "CK_Connection_DifferentUsers",
           "\"UserAId\" <> \"UserBId\"");

       entity.HasOne(x => x.UserA)
           .WithMany(x => x.ConnectionsAsUserA)
           .HasForeignKey(x => x.UserAId)
           .OnDelete(DeleteBehavior.Restrict);

       entity.HasOne(x => x.UserB)
           .WithMany(x => x.ConnectionsAsUserB)
           .HasForeignKey(x => x.UserBId)
           .OnDelete(DeleteBehavior.Restrict);
   });

        modelBuilder.Entity<ConnectionRequest>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.SenderUser)
                .WithMany(x => x.SentConnectionRequests)
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReceiverUser)
                .WithMany(x => x.ReceivedConnectionRequests)
                .HasForeignKey(x => x.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ConnectionId)
                .IsUnique();

            entity.HasOne(x => x.Connection)
                .WithOne(x => x.Conversation)
                .HasForeignKey<Conversation>(x => x.ConnectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Conversation)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SenderUser)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LearningSession>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Connection)
                .WithMany(x => x.LearningSessions)
                .HasForeignKey(x => x.ConnectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.LearningSessionId,
                x.RaterUserId
            }).IsUnique();

            entity.HasOne(x => x.LearningSession)
                .WithMany(x => x.Ratings)
                .HasForeignKey(x => x.LearningSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RaterUser)
                .WithMany(x => x.GivenRatings)
                .HasForeignKey(x => x.RaterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RatedUser)
                .WithMany(x => x.ReceivedRatings)
                .HasForeignKey(x => x.RatedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .HasConversion<string>()
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.UserId,
                x.IsRead,
                x.CreatedAt
            });

            entity.HasIndex(x => new
            {
                x.UserId,
                x.Type,
                x.RelatedEntityId
            }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Experience>(entity =>
{
    entity.HasKey(x => x.Id);

    entity.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Cascade);
});

        modelBuilder.Entity<SkillVerificationRequest>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.RequesterUser)
                .WithMany(x => x.RequestedSkillVerifications)
                .HasForeignKey(x => x.RequesterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MentorUser)
                .WithMany(x => x.MentoredSkillVerifications)
                .HasForeignKey(x => x.MentorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Skill)
                .WithMany(x => x.SkillVerificationRequests)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PointsTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.User)
                .WithMany(x => x.PointsTransactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Interest>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Name)
                .IsUnique();
        });

        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(x => new
            {
                x.UserId,
                x.SkillId
            });

            entity.HasOne(x => x.User)
                .WithMany(x => x.UserSkills)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Skill)
                .WithMany(x => x.UserSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserInterest>(entity =>
        {
            entity.HasKey(x => new
            {
                x.UserId,
                x.InterestId
            });

            entity.HasOne(x => x.User)
                .WithMany(x => x.UserInterests)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Interest)
                .WithMany(x => x.UserInterests)
                .HasForeignKey(x => x.InterestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningDirectionSkill>(entity =>
        {
            entity.HasKey(x => new
            {
                x.LearningDirectionId,
                x.SkillId
            });

            entity.HasOne(x => x.LearningDirection)
                .WithMany(x => x.LearningDirectionSkills)
                .HasForeignKey(x => x.LearningDirectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Skill)
                .WithMany(x => x.LearningDirectionSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningDirectionInterest>(entity =>
        {
            entity.HasKey(x => new
            {
                x.LearningDirectionId,
                x.InterestId
            });

            entity.HasOne(x => x.LearningDirection)
                .WithMany(x => x.LearningDirectionInterests)
                .HasForeignKey(x => x.LearningDirectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Interest)
                .WithMany(x => x.LearningDirectionInterests)
                .HasForeignKey(x => x.InterestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EducationalContentSkill>(entity =>
        {
            entity.HasKey(x => new
            {
                x.EducationalContentId,
                x.SkillId
            });

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.EducationalContentSkills)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Skill)
                .WithMany(x => x.EducationalContentSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EducationalContentInterest>(entity =>
        {
            entity.HasKey(x => new
            {
                x.EducationalContentId,
                x.InterestId
            });

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.EducationalContentInterests)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Interest)
                .WithMany(x => x.EducationalContentInterests)
                .HasForeignKey(x => x.InterestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EducationalContentLearningDirection>(entity =>
        {
            entity.HasKey(x => new
            {
                x.EducationalContentId,
                x.LearningDirectionId
            });

            entity.HasOne(x => x.EducationalContent)
                .WithMany(x => x.EducationalContentLearningDirections)
                .HasForeignKey(x => x.EducationalContentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.LearningDirection)
                .WithMany(x => x.EducationalContentLearningDirections)
                .HasForeignKey(x => x.LearningDirectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}