using AspProject.Domain.Abstractions;
using AspProject.Domain.Models;

namespace AspProject.Domain.Services;

public class FileService (IFileRepository repository): IFileService
{
    public async Task<bool> AddFiles(IFormFileCollection collection, Guid materialId)
    {
        if (await repository.AddFiles(collection, materialId))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public async Task<MaterialFilesDto> GetMaterialFiles(Guid materialId)
    {
        return await repository.GetFilesToMaterial(materialId);
    }
    
    public async Task<Tuple<byte[], string>> ReturnFilesByPath(string path, Guid materialId)
    {
        var result = await  repository.ReturnFileByPath(path, materialId);
        return result;
    }
    
    
}

