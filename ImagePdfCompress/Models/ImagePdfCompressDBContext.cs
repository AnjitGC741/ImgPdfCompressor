using Microsoft.EntityFrameworkCore;
namespace ImagePdfCompress.Models
{
    public class ImagePdfCompressDBContext : DbContext
    {
        public ImagePdfCompressDBContext(DbContextOptions<ImagePdfCompressDBContext> options) : base(options)
        {

        }
        public DbSet<CompressedFile> CompressedFiles { get; set; }
    }
}
