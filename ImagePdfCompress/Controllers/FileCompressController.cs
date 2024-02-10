using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using ImagePdfCompress.Models;
using System.Reflection.PortableExecutable;
using Spire.Pdf;
using Spire.Pdf.Conversion.Compression;
using Spire.Pdf.Graphics;
using System.Drawing;
using Microsoft.AspNetCore.Cors;
using Aspose.Pdf.Operators;
using System.IO.Compression;

namespace ImagePdfCompress.Controllers
{
    [EnableCors("MyPolicy")]
    public class FileCompressController : Controller
    {
        private readonly ImagePdfCompressDBContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public FileCompressController(ImagePdfCompressDBContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;

        }
        [HttpPost]
        [Route("compress-img")]
        public IActionResult CompressImages([FromForm] ImageCompressModel model)
        {
            try
            {
                if (model.ImageFiles == null || model.ImageFiles.Count == 0)
                {
                    return BadRequest("Invalid Image file");
                }

                if (model.ImageFiles.Count == 1)
                {
                    // Single file scenario
                    var compressedImageResult = CompressAndSaveImage(model.ImageFiles[0], model);
                    return compressedImageResult;
                }
                else
                {
                    // Multiple files scenario
                    var compressedZipFilePath = CompressAndSaveImages(model.ImageFiles, model);

                    var compressedZipFileInfo = new FileInfo(compressedZipFilePath);
                    var contentType = "application/zip";
                    var response = HttpContext.Response;

                    return new FileStreamResult(
                        new FileStream(compressedZipFileInfo.FullName, FileMode.Open), contentType)
                    {
                        FileDownloadName = "CompressedImages.zip"
                    };
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        private IActionResult CompressAndSaveImage(IFormFile imageFile, ImageCompressModel model)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return StatusCode(500, new { error = "Invalid File Format" });
                }

                var ImgName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var OriginalImageFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedImg/Original-Img");
                var OriginalImgPath = Path.Combine(OriginalImageFolderPath, ImgName);
                using (var stream = new FileStream(OriginalImgPath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                var CompressedImageFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedImg/Compressed-Img");
                var TempFilePath = Path.Combine(CompressedImageFolderPath, "temp_" + ImgName);
                using (var fileStream = new FileStream(TempFilePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }

                using (var imageStream = new FileStream(TempFilePath, FileMode.Open))
                {
                    using (var image = SixLabors.ImageSharp.Image.Load(imageStream))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new SixLabors.ImageSharp.Size(model.Width ?? 1024, model.Height ?? 600),
                            Mode = ResizeMode.Max
                        }));
                        image.Save(Path.Combine(CompressedImageFolderPath, ImgName), new JpegEncoder
                        {
                            Quality = model.Quality ?? 70
                        });
                    }
                }

                System.IO.File.Delete(TempFilePath);
                var compressedImageFileInfo = new FileInfo(Path.Combine(CompressedImageFolderPath, ImgName));
                var contentType = GetContentType(Path.GetExtension(ImgName).ToLowerInvariant());
                var response = HttpContext.Response;

                return new FileStreamResult(
                    new FileStream(compressedImageFileInfo.FullName, FileMode.Open), contentType)
                {
                    FileDownloadName = ImgName
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error compressing image", message = ex.Message });
            }
        }

        private string CompressAndSaveImages(List<IFormFile> imageFiles, ImageCompressModel model)
        {
            try
            {
                var compressedImageFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedImg/Compressed-Img");
                Directory.CreateDirectory(compressedImageFolderPath);

                var zipFileName = Guid.NewGuid().ToString() + ".zip";
                var compressedZipFilePath = Path.Combine(compressedImageFolderPath, zipFileName);

                using (var archive = ZipFile.Open(compressedZipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile == null || imageFile.Length == 0)
                        {
                            continue;
                        }

                        var ImgName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var TempFilePath = Path.Combine(compressedImageFolderPath, "temp_" + ImgName);

                        using (var fileStream = new FileStream(TempFilePath, FileMode.Create))
                        {
                            imageFile.CopyTo(fileStream);
                        }

                        using (var imageStream = new FileStream(TempFilePath, FileMode.Open))
                        {
                            using (var image = SixLabors.ImageSharp.Image.Load(imageStream))
                            {
                                image.Mutate(x => x.Resize(new ResizeOptions
                                {
                                    Size = new SixLabors.ImageSharp.Size(model.Width ?? 1024, model.Height ?? 600),
                                    Mode = ResizeMode.Max
                                }));
                                image.Save(Path.Combine(compressedImageFolderPath, ImgName), new JpegEncoder
                                {
                                    Quality = model.Quality ?? 70
                                });
                            }
                        }

                        archive.CreateEntryFromFile(Path.Combine(compressedImageFolderPath, ImgName), ImgName);
                        System.IO.File.Delete(TempFilePath);
                    }
                }

                return compressedZipFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception("Error compressing images", ex);
            }
        }
        [HttpPost]
        [Route("compress-pdf")]
        public IActionResult CompressPdf([FromForm] PdfCompressModel model)
        {
            try
            {
                if (model.PdfFiles== null || model.PdfFiles.Count == 0)
                {
                    return BadRequest("Invalid PDF file");
                }
                if (model.PdfFiles.Count == 1)
                {
                    var compressedPdfResult = CompressAndSavePdf(model.PdfFiles[0], model);
                    return compressedPdfResult;
                }
                else
                {
                    // Multiple files scenario
                    var compressedZipFilePath = CompressAndSavePdfs(model.PdfFiles, model);

                    var compressedZipFileInfo = new FileInfo(compressedZipFilePath);
                    var contentType = "application/zip";
                    var response = HttpContext.Response;

                    return new FileStreamResult(
                        new FileStream(compressedZipFileInfo.FullName, FileMode.Open), contentType)
                    {
                        FileDownloadName = "CompressedPdfs.zip"
                    };
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        private IActionResult CompressAndSavePdf(IFormFile pdfFile, PdfCompressModel model)
        {
            try
            {
                if (pdfFile == null || pdfFile.Length == 0)
                {
                    return StatusCode(500, new { error = "Invalid File Format" });
                }
                var pdfName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
                var originalPdfFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedPdf", "Original-Pdf");
                var originalPdfPath = Path.Combine(originalPdfFolderPath, pdfName);
                using (var stream = new FileStream(originalPdfPath, FileMode.Create))
                {
                    pdfFile.CopyTo(stream);
                }
                var compressedPdfFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedPdf", "Compressed-Pdf");
                var compressedPdfPath = Path.Combine(compressedPdfFolderPath, pdfName);
                PdfDocument doc = new PdfDocument(originalPdfPath);
                doc.FileInfo.IncrementalUpdate = false;
                foreach (PdfPageBase page in doc.Pages)
                {
                    System.Drawing.Image[] images = page.ExtractImages();
                    if (images != null && images.Length > 0)
                    {
                        for (int j = 0; j < images.Length; j++)
                        {
                            System.Drawing.Image image = images[j];
                            PdfBitmap bp = new PdfBitmap(image);
                            bp.Quality = 1;
                            page.ReplaceImage(j, bp);
                        }
                    }
                }
                doc.SaveToFile(compressedPdfPath);
                doc.Close();
                var compressedPdfFileInfo = new FileInfo(compressedPdfPath);
                var response = HttpContext.Response;

                return new FileStreamResult(
                    new FileStream(compressedPdfFileInfo.FullName, FileMode.Open), "application/pdf")
                {
                    FileDownloadName = "CompressPdf"
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error compressing image", message = ex.Message });
            }
        }
        private string CompressAndSavePdfs(List<IFormFile> pdfFiles, PdfCompressModel model)
        {
            try
            {
                var compressedPdfFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedPdf/Compressed-Pdf");
                Directory.CreateDirectory(compressedPdfFolderPath);
                var zipFileName = Guid.NewGuid().ToString() + ".zip";
                var compressedZipFilePath = Path.Combine(compressedPdfFolderPath, zipFileName);

                using (var archive = ZipFile.Open(compressedZipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var pdfFile in pdfFiles)
                    {
                        if (pdfFile == null || pdfFile.Length == 0)
                        {
                            continue;
                        }

                        var pdfName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
                        var tempFilePath = Path.Combine(compressedPdfFolderPath, "temp_" + pdfName);

                        using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                        {
                            pdfFile.CopyTo(fileStream);
                        }

                        using (PdfDocument doc = new PdfDocument(tempFilePath))
                        {
                            doc.FileInfo.IncrementalUpdate = false;

                            foreach (PdfPageBase page in doc.Pages)
                            {
                                System.Drawing.Image[] images = page.ExtractImages();
                                if (images != null && images.Length > 0)
                                {
                                    for (int j = 0; j < images.Length; j++)
                                    {
                                        System.Drawing.Image image = images[j];
                                        PdfBitmap bp = new PdfBitmap(image);
                                        bp.Quality = model.QualityLevel ?? 20;
                                        page.ReplaceImage(j, bp);
                                    }
                                }
                            }

                            var compressedPdfFilePath = Path.Combine(compressedPdfFolderPath, pdfName);
                            doc.SaveToFile(compressedPdfFilePath);
                            doc.Close();

                            archive.CreateEntryFromFile(compressedPdfFilePath, pdfName);
                            System.IO.File.Delete(tempFilePath);
                            System.IO.File.Delete(compressedPdfFilePath);
                        }
                    }
                }

                return compressedZipFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception("Error compressing PDFs", ex);
            }
        }

        private string GetContentType(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                default:
                    return "application/octet-stream";
            }
        }

    }
}
