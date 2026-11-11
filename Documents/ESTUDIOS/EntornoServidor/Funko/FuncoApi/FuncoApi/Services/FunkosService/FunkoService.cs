using FuncoApi.Cache.objet;
using FuncoApi.Repositories;

namespace FuncoApi.Services.FunkosService;

public class FunkoService : IFunkosService
{
    private ICacheService _cache;
    private IFunkoRepository _funkoRepository;
    
    public FunkoService(ICacheService cache, IFunkoRepository funkoRepository)
    {
        _cache = cache;
        _funkoRepository = funkoRepository;
    }
    
    
}