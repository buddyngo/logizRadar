using Logiz.Radar.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Logiz.Radar.Data.Context
{
    public class RadarContext : DbContext
    {
        public RadarContext(DbContextOptions<RadarContext> options) : base(options)
        {
        }

        public DbSet<Project> Project { get; set; }
        public DbSet<TestScenario> TestScenario { get; set; }
        public DbSet<TestVariant> TestVariant { get; set; }
        public DbSet<TestCase> TestCase { get; set; }
        public DbSet<TestCaseAttachment> TestCaseAttachment { get; set; }
        public DbSet<UserMappingProject> UserMappingProject { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().ToTable("Project");
            modelBuilder.Entity<TestScenario>().ToTable("TestScenario");
            modelBuilder.Entity<TestVariant>().ToTable("TestVariant");
            modelBuilder.Entity<TestCase>().ToTable("TestCase");
            modelBuilder.Entity<TestCaseAttachment>().ToTable("TestCaseAttachment");
            modelBuilder.Entity<UserMappingProject>().ToTable("UserMappingProject");
        }
    }
}