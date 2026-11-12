using FuncoApi.Entity;
using FuncoApi.Models;

namespace FuncoApi.Repositories.userRepository;

public interface IUserRepository
{
    public Task<UserEntity?> GetByEmailAsync(string email);

    public Task<UserEntity?> GetByIdAsync(Guid id);

    public Task<UserEntity> CreateAsync(UserEntity usuario);
    
    public Task<UserEntity> UpdateAsync(UserEntity usuario);
    
    public Task DeleteAsync(string id);
}