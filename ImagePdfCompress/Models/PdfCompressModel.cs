namespace ImagePdfCompress.Models
{
    public class PdfCompressModel
    {
        public List<IFormFile> PdfFiles { get; set; }
        public int? QualityLevel { get; set; }
    }
}
