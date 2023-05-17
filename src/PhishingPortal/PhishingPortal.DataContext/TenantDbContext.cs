using Microsoft.EntityFrameworkCore;
using PhishingPortal.Dto;

namespace PhishingPortal.DataContext
{
    public class TenantDbContext : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TenantDbContext(DbContextOptions<TenantDbContext> options, bool ensureCreated = true) : base(options)
        {
            if (ensureCreated)
                Database.EnsureCreated();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignDetail> CampaignDetails { get; set; }
        public DbSet<CampaignRecipient> CampaignRecipients { get; set; }

        public DbSet<CampaignLog> CampaignLogs { get; set; }
        public DbSet<FailedCampaignLog> FailedCampaignLogs { get; set; }

        public DbSet<CampaignTemplate> CampaignTemplates { get; set; }
        public DbSet<Metadata> MetaContents { get; set; }

        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<RecipientGroup> RecipientGroups { get; set; }
        public DbSet<RecipientGroupMapping> RecipientGroupMappings { get; set; }
        public DbSet<TenantSetting> Settings { get; set; }
        public DbSet<Training> Training { get; set; }
        public DbSet<TrainingLog> TrainingLog { get; set; }
        public DbSet<TrainingRecipients> TrainingRecipient { get; set; }
        public DbSet<TrainingCampaignMapping> TrainingCampaignMapping { get; set; }
        public DbSet<TrainingVideo> TrainingVideoPath { get; set; }
        public DbSet<TrainingQuiz> TrainingQuiz { get; set; }
        public DbSet<TrainingQuizAnswer> TrainingQuizAnswer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RecipientGroupMapping>().HasKey(o => new { o.GroupId, o.RecipientId });

        }

    }

}
