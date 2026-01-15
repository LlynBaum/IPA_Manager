using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Ipa.Manager.Services.Criterias;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;

namespace Ipa.Manager.Tests.UnitTests;

[TestFixture]
public class CriteriaProgressServiceTest
{
    [SetUp]
    public void Setup()
    {
        contextMock = new Mock<ApplicationDbContext>();
        staticCriteriaServiceMock = new Mock<IStaticCriteriaService>();
        service = new CriteriaProgressService(contextMock.Object, staticCriteriaServiceMock.Object);
    }

    private Mock<ApplicationDbContext> contextMock;
    private Mock<IStaticCriteriaService> staticCriteriaServiceMock;
    private CriteriaProgressService service;

    [Test]
    public async Task CreateAsync_CreatesCriteriaProgress_ForeachGivenCriteriaId()
    {
        contextMock.Setup(c => c.CriteriaProgress).ReturnsDbSet([]);
        
        await service.CreateAsync(1, ["1", "2"]);

        contextMock.Verify(
            c => c.CriteriaProgress.AddAsync(
                It.Is<CriteriaProgress>(cp => cp.CriteriaId == "1" && cp.ProjectId == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
        contextMock.Verify(
            c => c.CriteriaProgress.AddAsync(
                It.Is<CriteriaProgress>(cp => cp.CriteriaId == "2" && cp.ProjectId == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetByProject_ReturnsExpectedProjectCriterias()
    {
        staticCriteriaServiceMock.Setup(s => s.GetById(It.IsAny<string>()))
            .Returns(() => new Criteria("1","1", "1", ["1"]));
        
        contextMock
            .Setup(c => c.CriteriaProgress)
            .ReturnsDbSet([
                new CriteriaProgress
                {
                    Id = 1,
                    ProjectId = 1,
                    CriteriaId = "1"
                },
                new CriteriaProgress
                {
                    Id = 2,
                    ProjectId = 2,
                    CriteriaId = "2"
                }
            ]);

        var result = await service.GetByProject(1);
        
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result.Single().Criteria.Id, Is.EqualTo("1"));
            Assert.That(result.Single().CriteriaProgress.Id, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetById_ReturnsExpectedCriteria()
    {
        staticCriteriaServiceMock.Setup(s => s.GetById(It.IsAny<string>()))
            .Returns(() => new Criteria("1","1", "1", ["1"]));
        
        contextMock
            .Setup(c => c.CriteriaProgress)
            .ReturnsDbSet([
                new CriteriaProgress
                {
                    Id = 1,
                    ProjectId = 1,
                    CriteriaId = "1"
                },
                new CriteriaProgress
                {
                    Id = 2,
                    ProjectId = 2,
                    CriteriaId = "2"
                }
            ]);

        var result = await service.GetById(1);
        
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Criteria.Id, Is.EqualTo("1"));
            Assert.That(result.CriteriaProgress.Id, Is.EqualTo(1));
        });
    }
}