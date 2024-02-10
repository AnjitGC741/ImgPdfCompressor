namespace ImagePdfCompress.Models
{
    public class ImageCompressModel
    {
        public List<IFormFile> ImageFiles { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Quality { get; set; }
    }
}
