
using EduShpere.Domain;
using EduShpere.Domain.Models;
namespace EduShpere.Application;
public interface IHttpContextService
{
    Task<User?> GetAppUser();
    Task<User> GetAppUserAndThrow();
    string GetIpAddress();
}