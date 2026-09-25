using AspProject.Domain.Entities;
using AspProject.Domain.Models;

namespace AspProject.Domain.Abstractions;

public interface IFileService
{
    public Task<bool> AddFiles(IFormFileCollection collection, Guid materialId);
    public Task<MaterialFilesDto> GetMaterialFiles(Guid materialId);
    public Task<Tuple<byte[], string>> ReturnFilesByPath(string path, Guid materialId);
}
