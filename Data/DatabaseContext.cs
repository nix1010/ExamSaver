using ExamSaver.Configs;
using ExamSaver.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExamSaver.Data
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration configuration;

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration) : base(options)
        {
            this.configuration = configuration;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<StudentExam> StudentsExams { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<UserSubject> UsersSubjects { get; set; }
        public DbSet<SimilarityResult> SimilarityResults { get; set; }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<UserSubject>()
                .HasKey(us => new { us.UserId, us.SubjectId });

            modelBuilder
                .Entity<UserSubject>()
                .Property(us => us.SubjectRelation)
                .HasConversion<string>();

            modelBuilder
                .Entity<StudentExam>()
                .HasKey(se => new { se.StudentId, se.ExamId });
        }
    }
}
