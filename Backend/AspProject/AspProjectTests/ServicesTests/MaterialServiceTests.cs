using AspProject.Api.Models;
using AspProject.Domain.Abstractions;
using AspProject.Domain.Models;
using AspProject.Domain.Services;
using AutoMapper;
using Moq;
using Xunit;

namespace AspProjectTests.ServicesTests;

public class MaterialsServiceTests
{
    private readonly Mock<IMaterialRepository> _mockMaterialRepository = new();
    private readonly Mock<IFileService> _mockFileService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly MaterialsService _materialsService;

    public MaterialsServiceTests()
    {
        _materialsService = new MaterialsService(
            _mockMaterialRepository.Object,
            _mockFileService.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task SaveMaterial_ShouldReturnTrue_WhenRepositorySucceeds()
    {
        // Arrange
        var materialDto = new MaterialDto();
        _mockMaterialRepository.Setup(r => r.AddMaterial(materialDto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _materialsService.SaveMaterial(materialDto);

        // Assert
        Assert.True(result);
        _mockMaterialRepository.Verify(r => r.AddMaterial(materialDto), Times.Once);
    }
    
    [Fact]
    public async Task GetMaterialsByFilters_ShouldReturnMaterials_WhenFiltersValid()
    {
        var filters = new FiltersDto(1, "Math", "John Doe", 1);
        var expectedMaterials = new List<MaterialResponseDto>
        {
            new MaterialResponseDto { Subject = "Math" }
        };
        
        _mockMaterialRepository.Setup(r => r.GetMaterialsByFilters(filters))
            .ReturnsAsync(expectedMaterials);

        var result = await _materialsService.GetMaterialsByFilters(filters);

        Assert.Equal(expectedMaterials, result);
        Assert.Equal("Math", result?[0].Subject);
    }
    

    [Fact]
    public async Task GetMaterialById_ShouldReturnMaterial_WhenExists()
    {
        var materialId = Guid.NewGuid();
        var expectedMaterial = new MaterialResponseDto { MaterialId = materialId };
        
        _mockMaterialRepository.Setup(r => r.GetMaterialById(materialId))
            .ReturnsAsync(expectedMaterial);

        var result = await _materialsService.GetMaterialById(materialId);

        Assert.Equal(expectedMaterial, result);
        Assert.Equal(materialId, result?.MaterialId);
    }
    
    [Fact]
    public async Task GetMaterialById_ShouldReturnNull_WhenNotFound()
    {
        var materialId = Guid.NewGuid();
        _mockMaterialRepository.Setup(r => r.GetMaterialById(materialId))
            .ReturnsAsync((MaterialResponseDto?)null);

        var result = await _materialsService.GetMaterialById(materialId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMaterialsByStudentId_ShouldReturnMaterials_WhenStudentExists()
    {
        var studentId = Guid.NewGuid();
        var expectedMaterials = new List<MaterialResponseDto>
        {
            new MaterialResponseDto { StudentId = studentId }
        };
        
        _mockMaterialRepository.Setup(r => r.GetMaterialsByStudent(studentId))
            .ReturnsAsync(expectedMaterials);

        var result = await _materialsService.GetMaterialsByStudentId(studentId);

        Assert.Equal(studentId, result?[0].StudentId);
    }

    [Fact]
    public async Task GetMaterialsByStudentId_ShouldReturnNull_WhenRepositoryReturnsNull()
    {
        var studentId = Guid.NewGuid();
        _mockMaterialRepository.Setup(r => r.GetMaterialsByStudent(studentId))
            .ReturnsAsync((List<MaterialResponseDto>?)null);

        var result = await _materialsService.GetMaterialsByStudentId(studentId);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteMaterialById_ShouldReturnTrue_WhenDeletionSucceeds()
    {
        var materialId = Guid.NewGuid();
        _mockMaterialRepository.Setup(r => r.DeleteMaterialById(materialId))
            .ReturnsAsync(true);

        var result = await _materialsService.DeleteMaterialById(materialId);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteMaterialById_ShouldReturnFalse_WhenDeletionFails()
    {
        var materialId = Guid.NewGuid();
        _mockMaterialRepository.Setup(r => r.DeleteMaterialById(materialId))
            .ReturnsAsync(false);

        var result = await _materialsService.DeleteMaterialById(materialId);

        Assert.False(result);
    }
    

    [Fact]
    public async Task GetMaterialsByKeyWord_ShouldReturnMaterials_WhenKeywordMatches()
    {
        var keyword = "math";
        var expectedMaterials = new List<MaterialResponseDto>
        {
            new MaterialResponseDto { Subject = "Mathematics" }
        };
        
        _mockMaterialRepository.Setup(r => r.GetMaterialsByKeyWords(keyword))
            .ReturnsAsync(expectedMaterials);

        var result = await _materialsService.GetMaterialsByKeyWord(keyword);

        Assert.Equal("Mathematics", result?[0].Subject);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetMaterialsByKeyWord_ShouldHandleEmptyKeywords(string keyword)
    {
        var expectedMaterials = new List<MaterialResponseDto>();
        _mockMaterialRepository.Setup(r => r.GetMaterialsByKeyWords(It.IsAny<string>()))
            .ReturnsAsync(expectedMaterials);

        var result = await _materialsService.GetMaterialsByKeyWord(keyword);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}