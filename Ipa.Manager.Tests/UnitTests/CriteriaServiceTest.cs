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
            await service.InitializeAsync(FileName);
            
            // Assert
            var result = service.GetAll();
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].Id, Is.EqualTo("A01"));
                Assert.That(result[1].Id, Is.EqualTo("A02"));
                Assert.That(result[2].Id, Is.EqualTo("A03"));
            });
        }
    }
    
    private class UsageTests
    {
        private readonly CriteriaService service = new CriteriaService();

        [OneTimeSetUp]
        public async Task SetUp()
        {
            await service.InitializeAsync(FileName);
        }
        
        [Test]
        public void GetAll_ReturnsAllCriteria()
        {
            // Act
            var result = service.GetAll();
            
            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].Id, Is.EqualTo("A01"));
                Assert.That(result[1].Id, Is.EqualTo("A02"));
                Assert.That(result[2].Id, Is.EqualTo("A03"));
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
        
        [Test]
        public void GetById_ThrowsException_WhenCriteriaWasNotFound()
        {
            // Act
            void Act() => _ = service.GetById("KNFE");

            // Assert
            Assert.Throws<KeyNotFoundException>(Act);
        }
        
        [Test]
        public void GetAllById_ReturnsExpectedCrieterias()
        {
            // Act
            var result = service.GetAllById(["A01", "A03"]);
            
            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].Id, Is.EqualTo("A01"));
                Assert.That(result[1].Id, Is.EqualTo("A03"));
            });
        }
        
        [Test]
        public void GetAllById_ReturnsEmptyList_WhenNoCriteriaIdIsGiven()
        {
            // Act
            var result = service.GetAllById([]);
            
            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }
        
        [Test]
        public void GetLookupTableByIds_ReturnsExpectedCrieterias()
        {
            // Act
            var result = service.GetLookupTableByIds(["A01", "A03"]);
            
            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result, Does.ContainKey("A01"));
                Assert.That(result["A01"].Id, Is.EqualTo("A01"));
                
                Assert.That(result, Does.ContainKey("A03"));
                Assert.That(result["A03"].Id, Is.EqualTo("A03"));
            });
        }
        
        [Test]
        public void GetLookupTableByIds_ReturnsEmptyList_WhenNoCriteriaIdIsGiven()
        {
            // Act
            var result = service.GetLookupTableByIds([]);
            
            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }
    }
}