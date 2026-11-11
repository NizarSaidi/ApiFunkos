using FuncoApi.Domain;
using FuncoApi.Models;
using FuncoApi.Services.objet;

namespace FuncoApi.Services.FunkosService;

/// <summary>Contrato de operaciones de aplicación para Funkos.</summary>
public interface IFunkosService : IService<Funko, int>
{
}
