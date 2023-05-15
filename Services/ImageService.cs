using CrucibleBlog.Services.Interfaces;

namespace CrucibleBlog.Services
{
    public class ImageService : IImageService
    {
        private readonly string? _defaultUserImage = "/img/DefaultUserImage.jpg";
        private readonly string? _defaultBlogImage = "/img/DefaultBlogImage.jpg";
        private readonly string? _defaultCategoryImage = "/img/DefaultCategoryImage.jpg";
        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, int defaultImage)
        {
            if (fileData == null || fileData.Length == 0) //no byte info, return default image 
            {
                switch (defaultImage)
                {
                    //Return The default user image if the value is 1
                    case 1: return _defaultUserImage;
                    //Return The default blog image if the value is 2
                    case 2: return _defaultBlogImage;
                    //Return The default cat image if the value is 2
                    case 3: return _defaultCategoryImage;

                }
            }

            try
            {
                string? imageBase64Data = Convert.ToBase64String(fileData!);
                imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}"); //this makes it an image in the view

                return imageBase64Data;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]?> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file!.CopyToAsync(memoryStream); //copying iformfile into memory stream
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
