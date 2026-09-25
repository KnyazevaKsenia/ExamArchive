using AspProject.Api.Models;
using AspProject.Domain.Abstractions.IExamImitation;
using AspProject.Domain.Models;
using AspProject.Domain.Services.ExamImitation;
using Moq;
using Xunit;

namespace AspProjectTests.ServicesTests;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _mockTicketRepository = new();
    private readonly TicketService _ticketService;

    public TicketServiceTests()
    {
        _ticketService = new TicketService(_mockTicketRepository.Object);
    }
    
    [Fact]
    public async Task CreateTicketsSet_ShouldCreateEvenPairs_WhenEvenQuestions()
    {
        var request = new CreateTicketRequest
        {
            SubjectName = "Math",
            Duration = new TimeOnly(1, 30),
            Questions = new List<string> { "Q1", "Q2", "Q3", "Q4" }
        };
        var studentId = Guid.NewGuid();

        _mockTicketRepository.Setup(r => r.SaveTicketSet(It.IsAny<TicketSetDto>(), studentId))
            .ReturnsAsync(true);

        var result = await _ticketService.CreateTicketsSet(request, studentId);

        Assert.True(result);
        _mockTicketRepository.Verify(r => r.SaveTicketSet(It.Is<TicketSetDto>(s => 
            s.Tickets.Count == 2 && 
            s.Tickets[0].FirstQuestion == "Q1" &&
            s.Tickets[0].SecondQuestion == "Q3" &&
            s.Tickets[1].FirstQuestion == "Q2" &&
            s.Tickets[1].SecondQuestion == "Q4" &&
            s.Subject == "Math" &&
            s.Duration == new TimeOnly(1, 30)), studentId), Times.Once);
    }

    [Fact]
    public async Task CreateTicketsSet_ShouldHandleOddQuestions_WithEmptySecondQuestion()
    {
        var request = new CreateTicketRequest
        {
            SubjectName = "Physics",
            Duration = new TimeOnly(1, 0),
            Questions = new List<string> { "Q1", "Q2", "Q3" }
        };
        var studentId = Guid.NewGuid();

        _mockTicketRepository.Setup(r => r.SaveTicketSet(It.IsAny<TicketSetDto>(), studentId))
            .ReturnsAsync(true);

        var result = await _ticketService.CreateTicketsSet(request, studentId);

        Assert.True(result);
        _mockTicketRepository.Verify(r => r.SaveTicketSet(It.Is<TicketSetDto>(s => 
            s.Tickets.Count == 2 &&
            s.Tickets[1].FirstQuestion == "Q3" &&
            s.Tickets[1].SecondQuestion == "" &&
            s.Subject == "Physics" &&
            s.Duration == new TimeOnly(1, 0)), studentId), Times.Once);
    }

    [Fact]
    public async Task CreateTicketsSet_ShouldHandleSingleQuestion()
    {
        var request = new CreateTicketRequest
        {
            SubjectName = "Chemistry",
            Duration = new TimeOnly(0, 45),
            Questions = new List<string> { "Q1" }
        };
        var studentId = Guid.NewGuid();

        _mockTicketRepository.Setup(r => r.SaveTicketSet(It.IsAny<TicketSetDto>(), studentId))
            .ReturnsAsync(true);

        var result = await _ticketService.CreateTicketsSet(request, studentId);

        Assert.True(result);
        _mockTicketRepository.Verify(r => r.SaveTicketSet(It.Is<TicketSetDto>(s => 
            s.Tickets.Count == 1 &&
            s.Tickets[0].FirstQuestion == "Q1" &&
            s.Tickets[0].SecondQuestion == "" &&
            s.Subject == "Chemistry" &&
            s.Duration == new TimeOnly(0, 45)), studentId), Times.Once);
    }

    [Fact]
    public async Task GetTicketSets_ShouldReturnTicketSets_WhenTheyExist()
    {
        var studentId = Guid.NewGuid();
        var expectedSets = new List<TicketSetDto>
        {
            new TicketSetDto { 
                Id = Guid.NewGuid(),
                Subject = "Math",
                Tickets = new List<TicketDto>(),
                Duration = new TimeOnly(1, 0)
            },
            new TicketSetDto {
                Id = Guid.NewGuid(),
                Subject = "Physics",
                Tickets = new List<TicketDto>(),
                Duration = new TimeOnly(1, 30)
            }
        };

        _mockTicketRepository.Setup(r => r.GetAllStudentTicketSets(studentId))
            .ReturnsAsync(expectedSets);

        var result = await _ticketService.GetTicketSets(studentId);

        Assert.Equal(2, result.Count);
        Assert.Equal("Math", result[0].Subject);
        Assert.Equal("Physics", result[1].Subject);
    }

    [Fact]
    public async Task GetTicketSets_ShouldReturnEmptyList_WhenNoSetsExist()
    {
        var studentId = Guid.NewGuid();

        _mockTicketRepository.Setup(r => r.GetAllStudentTicketSets(studentId))
            .ReturnsAsync(new List<TicketSetDto>());

        var result = await _ticketService.GetTicketSets(studentId);

        Assert.Empty(result);
    }

    

}