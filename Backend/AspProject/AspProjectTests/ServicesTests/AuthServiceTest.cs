using AspProject.Api.Models;
using AspProject.Domain.Abstractions.Auth;
using AspProject.Domain.Models;
using AspProject.Domain.Services;
using Moq;
using Xunit;

namespace AspProjectTests.ServicesTests;
public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _mockAuthRepository;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockAuthRepository = new Mock<IAuthRepository>();
        _authService = new AuthService(_mockAuthRepository.Object);
    }
    
    [Fact]
    public async Task AddUser_ShouldCallRepositoryAndReturnResult()
    {
        var model = new RegistrationModel
        {
            Login = "test@example.com",
            Password = "password123",
            FirstName = "Masha",
            LastName = "Mashevna"
        };
    
        _mockAuthRepository.Setup(r => r.AddNewUserAsync(model))
            .ReturnsAsync(true);

        var result = await _authService.AddUser(model);

        Assert.True(result);
        _mockAuthRepository.Verify(r => r.AddNewUserAsync(model), Times.Once);
    }
    
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("nonexistent@example.com", false)]
    public async Task IsUserExist_ShouldCallRepositoryAndReturnResult(string email, bool expected)
    {
        _mockAuthRepository.Setup(r => r.IsUserExist(email))
            .ReturnsAsync(expected);

        var result = await _authService.IsUserExist(email);

        Assert.Equal(expected, result);
        _mockAuthRepository.Verify(r => r.IsUserExist(email), Times.Once);
    }
    
    [Fact]
    public async Task AuthorizeUser_ShouldReturnStudentDto_WhenCredentialsValid()
    {
        var request = new LoginRequest { Login = "test@example.com", Password = "password123" };
        var expectedStudent = new StudentDto { Id = Guid.NewGuid() };
    
        _mockAuthRepository.Setup(r => r.Authorize(request))
            .ReturnsAsync(expectedStudent);
        
        var result = await _authService.AuthorizeUser(request);
        
        Assert.Equal(expectedStudent, result);
        _mockAuthRepository.Verify(r => r.Authorize(request), Times.Once);
    }
    
    [Fact]
    public async Task AuthorizeUser_ShouldReturnNull_WhenCredentialsInvalid()
    {
        var request = new LoginRequest { Login = "test@example.com", Password = "wrong_password" };
    
        _mockAuthRepository.Setup(r => r.Authorize(request))
            .ReturnsAsync((StudentDto?)null);
        
        var result = await _authService.AuthorizeUser(request);
     
        Assert.Null(result);
        _mockAuthRepository.Verify(r => r.Authorize(request), Times.Once);
    }
    [Fact]
    public async Task GetUserIdByStudentId_ShouldReturnUserId_WhenStudentExists()
    {
        var studentId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
    
        _mockAuthRepository.Setup(r => r.GetUserByStudentId(studentId))
            .ReturnsAsync(expectedUserId);

        var result = await _authService.GetUserIdByStudentId(studentId);

        Assert.Equal(expectedUserId, result);
        _mockAuthRepository.Verify(r => r.GetUserByStudentId(studentId), Times.Once);
    }

    [Fact]
    public async Task GetUserIdByStudentId_ShouldReturnEmptyGuid_WhenStudentNotFound()
    {
        var studentId = Guid.NewGuid();
    
        _mockAuthRepository.Setup(r => r.GetUserByStudentId(studentId))
            .ReturnsAsync(Guid.Empty);
        
        var result = await _authService.GetUserIdByStudentId(studentId);
        
        Assert.Equal(Guid.Empty, result);
    }
    
    [Fact]
    public async Task GetStudentByRefresh_ShouldReturnStudentDto_WhenTokenValid()
    {
        var refreshToken = "valid_token";
        var expectedStudent = new StudentDto { Id = Guid.NewGuid() };
    
        _mockAuthRepository.Setup(r => r.GetUserByRefreshToken(refreshToken))
            .ReturnsAsync(expectedStudent);

        var result = await _authService.GetStudentByRefresh(refreshToken);

        Assert.Equal(expectedStudent, result);
        _mockAuthRepository.Verify(r => r.GetUserByRefreshToken(refreshToken), Times.Once);
    }

    [Fact]
    public async Task GetStudentByRefresh_ShouldReturnNull()
    {
        var refreshToken = "invalid_token";
    
        _mockAuthRepository.Setup(r => r.GetUserByRefreshToken(refreshToken))
            .ReturnsAsync((StudentDto?)null);
        
        var result = await _authService.GetStudentByRefresh(refreshToken);

        Assert.Null(result);
    }
    
    
}