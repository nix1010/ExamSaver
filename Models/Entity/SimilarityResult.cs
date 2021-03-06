using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.Entity
{
    [Table("similarity_results")]
    public class SimilarityResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("exam_id")]
        [ForeignKey("Exam")]
        public int ExamId { get; set; }
        public Exam Exam { get; set; }
        
        [Column("url")]
        public string Url { get; set; }

        [Column("comment")]
        public string Comment { get; set; }
        
        [Column("submit_time")]
        public DateTime Submitted { get; set; }

    }
}
