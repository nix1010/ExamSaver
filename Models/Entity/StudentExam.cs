using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.Entity
{
    [Table("students_exams")]
    public class StudentExam
    {
        [Column("student_id")]
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }
        
        [Column("exam_id")]
        [ForeignKey("Exam")]
        public int ExamId { get; set; }
        public Exam Exam { get; set; }
        [Column("upload_time")]
        public DateTime UploadTime { get; set; }
        [Column("exam_path")]
        public string ExamPath { get; set; }
    }
}
