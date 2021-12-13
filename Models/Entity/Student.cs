using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.Entity
{
    [Table("students")]
    public class Student
    {
        [Key]
        [Column("user_id")]
        [ForeignKey("User")]
        public int Id { get; set; }
        public User User { get; set; }

        [Column("index")]
        public string Index { get; set; }

        public ICollection<StudentExam> Exams { get; set; }
    }
}
