using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ImagePdfCompress.Models
{
    public class CompressedFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FileFormat {  get; set; }   
        public long OriginalSize { get; set; }
        public long CompressedSize { get; set; }
        public int CompressedPercentage { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public int? Quality { get; set; }
        public string OriginalFilePath { get; set; }
        public string CompressedFilePath { get; set; }
        public DateTime CompressedDate { get; set; } = DateTime.Now;
    }
   
}
