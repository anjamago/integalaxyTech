using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Application.Services;
using IntergalaxyTech.Domain.Entities;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace IntergalaxyTech.Tests.Application;

public class PersonajeServiceTests
{
    private readonly Mock<IRickAndMortyApiClient> _apiClientMock;
    private readonly Mock<IPersonajeRepository> _personajeRepositoryMock;
    private readonly Mock<ILogger<PersonajeService>> _loggerMock;
    private readonly PersonajeService _sut;

    public PersonajeServiceTests()
    {
        _apiClientMock = new Mock<IRickAndMortyApiClient>();
        _personajeRepositoryMock = new Mock<IPersonajeRepository>();
        _loggerMock = new Mock<ILogger<PersonajeService>>();
        _sut = new PersonajeService(_apiClientMock.Object, _personajeRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SyncPersonajesAsync_ShouldOnlyAddNonExistingCharacters()
    {
        var externalCharacters = new List<Personaje>
        {
            new Personaje { Id = 1, Nombre = "Rick Sanchez" },
            new Personaje { Id = 2, Nombre = "Morty Smith" }
        };

        var existingCharacters = new List<Personaje>
        {
            new Personaje { Id = 1, Nombre = "Rick Sanchez" }
        };

        _apiClientMock.Setup(client => client.GetCharactersAsync(1))
            .ReturnsAsync(externalCharacters);

        _personajeRepositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(existingCharacters);

        await _sut.SyncPersonajesAsync();

        _personajeRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Personaje>(p => p.Id == 2)), Times.Once);
        _personajeRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Personaje>(p => p.Id == 1)), Times.Never);
    }
}
