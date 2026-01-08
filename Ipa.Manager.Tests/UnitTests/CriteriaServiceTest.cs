using Ipa.Manager.Services.Criterias;
using NUnit.Framework;

namespace Ipa.Manager.Tests.UnitTests;

[TestFixture]
public class CriteriaServiceTest
{
    private const string FileName = "UnitTests/criteria.json";
    
    private class InitTest
    {
        [Test]
        public async Task InitializeAsync_LoadsAllCriteria()
        { 
            // Arrange
            var service = new CriteriaService(); 
            
            // Act
            await service.InitializeAsync(GetType().Assembly, FileName);
            
            // Assert
            var result = service.GetAll();
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].Id, Is.EqualTo("A01"));
                Assert.That(result[1].Id, Is.EqualTo("A02"));
            });
        }
    }
    
    private class UsageTests
    {
        private readonly CriteriaService service = new CriteriaService();

        [OneTimeSetUp]
        public async Task SetUp()
        {
            await service.InitializeAsync(GetType().Assembly, FileName);
        }
        
        [Test]
        public void GetAll_ReturnsAllCriteria()
        {
            // Act
            var result = service.GetAll();
            
            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].Id, Is.EqualTo("A01"));
                Assert.That(result[1].Id, Is.EqualTo("A02"));
            });
        }
        
        [Test]
        public void GetById_ReturnsExpectedCriteria()
        {
            // Act
            var result = service.GetById("A01");
            
            // Assert
            Assert.That(result.Id, Is.EqualTo("A01"));
        }
    }
}