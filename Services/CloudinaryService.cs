using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Toyo_cable_UI.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService()
        {
            Account account = new Account(
                "dln2oecnw",
                "177734427929634",
                "leF7kU84BfhIukeut_Kz_K-UJto"
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string?> UploadImageAsync(StorageFile imageFile)
        {
            try
            {
                // Read file as stream
                using var stream = await imageFile.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.AsStreamForRead().CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Upload to Cloudinary
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(imageFile.Name, memoryStream),
                    Folder = "toyo-cable/products", 
                    PublicId = $"product_{Guid.NewGuid()}", 
                    Overwrite = false,
                    Transformation = new Transformation()
                        .Width(800).Height(800).Crop("limit") 
                };

                ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Return the secure URL
                    return uploadResult.SecureUrl.ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cloudinary upload error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                // Extract public ID from URL
                var uri = new Uri(imageUrl);
                var publicId = Path.GetFileNameWithoutExtension(uri.AbsolutePath);

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                return result.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cloudinary delete error: {ex.Message}");
                return false;
            }
        }
    }
}