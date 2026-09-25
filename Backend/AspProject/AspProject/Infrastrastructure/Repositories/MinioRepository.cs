using System.Net;
using AspProject.Domain.Abstractions;
using AspProject.Domain.Models;
using Microsoft.AspNetCore.StaticFiles;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace AspProject.Infrastrastructure.Repositories;

public class MinioRepository: IFileRepository
{
    public IMinioClient MinioClient { get; }

    public MinioRepository(IMinioClient minioClientClient)
    {
        MinioClient = minioClientClient;
    }
    
    public async Task<bool> AddFiles(IFormFileCollection collection, Guid materialId)
    {
        try {
            foreach (var file in collection)
            {
                await MinioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket("material-files")
                        .WithContentType(file.ContentType)
                        .WithObject($"{materialId}/{file.FileName}")
                        .WithStreamData(file.OpenReadStream())
                        .WithObjectSize(-1),
                    CancellationToken.None);
            }
            
            return true;
        }
        catch(Exception ex) {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
        
    public async Task<MaterialFilesDto> GetFilesToMaterial(Guid materialId)
    {
        var result = new MaterialFilesDto
        {
            FileNameUrl = new Dictionary<string, string>(),
            ImagesNameUrl = new Dictionary<string, string>()
        };
        string objectPrefix = $"{materialId}/";
        var args = new ListObjectsArgs()
            .WithBucket("material-files")
            .WithPrefix(objectPrefix)
            .WithRecursive(false);
        
        bool bucketExists = await MinioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket("material-files"));
        
        var minioFiles = MinioClient.ListObjectsEnumAsync(args);
        
        var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };
        
        await foreach (var item in minioFiles)
        {
            var fileName = Path.GetFileName(item.Key);

            var presignedUrl = await MinioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket("material-files")
                    .WithObject(item.Key)
                    .WithExpiry(3600));
            
            var extension = Path.GetExtension(fileName);
            
            if (imageExtensions.Contains(extension))
            {
                result.ImagesNameUrl[fileName] = presignedUrl;
            }
            else
            {
                result.FileNameUrl[fileName] = presignedUrl;
            }
        }
        
        return result;
    }
    public async Task<Tuple<byte[], string>> ReturnFileByPath(string fileName, Guid materialId)
    {
        string bucketName = "material-files";
        string objectName = $"{materialId}/{fileName}";
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await MinioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream)));
            
                var fileBytes = memoryStream.ToArray();

                var provider = new FileExtensionContentTypeProvider();
                string contentType;
                if (!provider.TryGetContentType(fileName, out contentType))
                {
                    contentType = "application/octet-stream";
                }
                
                return new Tuple<byte[], string>(fileBytes, contentType);
            }
        }
        catch (MinioException ex)
        {
            return null;
        }
    }
}


