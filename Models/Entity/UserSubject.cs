using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.Entity
{
    [Table("users_subjects")]
    public class UserSubject
    {
        [Column("user_id")]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Column("subject_id")]
        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        
        [Column("subject_relation")]
        public SubjectRelationType SubjectRelation { get; set; }
    }
}
