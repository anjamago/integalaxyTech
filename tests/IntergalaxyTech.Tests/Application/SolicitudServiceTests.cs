using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Application.Services;
using IntergalaxyTech.Domain.Entities;
using IntergalaxyTech.Domain.Enums;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Moq;
using Xunit;

namespace IntergalaxyTech.Tests.Application;

public class SolicitudServiceTests
{
    private readonly Mock<ISolicitudRepository> _solicitudRepositoryMock;
    private readonly Mock<IRepository<Personaje>> _personajeRepositoryMock;
    private readonly Mock<ILogger<SolicitudService>> _loggerMock;
    private readonly Mock<IValidator<CrearSolicitudDto>> _validatorMock;
    private readonly SolicitudService _sut;

    public SolicitudServiceTests()
    {
        _solicitudRepositoryMock = new Mock<ISolicitudRepository>();
        _personajeRepositoryMock = new Mock<IRepository<Personaje>>();
        _loggerMock = new Mock<ILogger<SolicitudService>>();
        _validatorMock = new Mock<IValidator<CrearSolicitudDto>>();

        _sut = new SolicitudService(
            _solicitudRepositoryMock.Object, 
            _personajeRepositoryMock.Object,
            _loggerMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task CrearSolicitud_ConPersonajeInexistente_DeberiaLanzarArgumentException()
    {
        var request = new CrearSolicitudDto { PersonajeId = 999, Solicitante = "Test", AccionRequerida = "Prueba" };
        _personajeRepositoryMock.Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Personaje?)null);

        Func<Task> action = async () => await _sut.CrearSolicitudAsync(request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Personaje no encontrado en la base de datos local.");
    }

    [Fact]
    public async Task CrearSolicitud_ConPersonajeExistente_DeberiaLlamarAddAsyncYRetornarDto()
    {
        var request = new CrearSolicitudDto { PersonajeId = 1, Solicitante = "Luna", AccionRequerida = "Investigar" };
        var personajeMock = new Personaje { Id = 1, Nombre = "Rick Sanchez" };

        _personajeRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(personajeMock);

        var result = await _sut.CrearSolicitudAsync(request);

        result.Should().NotBeNull();
        result.Solicitante.Should().Be("Luna");
        result.Estado.Should().Be(EstadoSolicitud.Pendiente);
        _solicitudRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Solicitud>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarEstado_PendienteAAprobada_DeberiaLanzarExcepcion()
    {
        var id = Guid.NewGuid();
        var solicitudMock = new Solicitud { Id = id, Estado = EstadoSolicitud.Pendiente };

        _solicitudRepositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(solicitudMock);

        var peticion = new ActualizarEstadoSolicitudDto { Estado = EstadoSolicitud.Aprobada };

        Func<Task> action = async () => await _sut.ActualizarEstadoAsync(id, peticion);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Solo se permite pasar de Pendiente a EnProceso o Rechazada.");
    }

    [Fact]
    public async Task ActualizarEstado_DePendienteARechazadaSinMotivo_DeberiaLanzarExcepcion()
    {
        var id = Guid.NewGuid();
        var solicitudMock = new Solicitud { Id = id, Estado = EstadoSolicitud.Pendiente };

        _solicitudRepositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(solicitudMock);

        var peticion = new ActualizarEstadoSolicitudDto { Estado = EstadoSolicitud.Rechazada, MotivoRechazo = "" };

        Func<Task> action = async () => await _sut.ActualizarEstadoAsync(id, peticion);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Se requiere proveer un motivo (MotivoRechazo) para Rechazarlas.");
    }

    [Fact]
    public async Task ActualizarEstado_DePendienteAEnProceso_DeberiaLlamarUpdateAsync()
    {
        var id = Guid.NewGuid();
        var solicitudMock = new Solicitud { Id = id, Estado = EstadoSolicitud.Pendiente };

        _solicitudRepositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(solicitudMock);

        var peticion = new ActualizarEstadoSolicitudDto { Estado = EstadoSolicitud.EnProceso };

        await _sut.ActualizarEstadoAsync(id, peticion);

        solicitudMock.Estado.Should().Be(EstadoSolicitud.EnProceso);
        _solicitudRepositoryMock.Verify(repo => repo.UpdateAsync(solicitudMock), Times.Once);
    }
}
