using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces
{
    public interface ILoggedInUserService
    {
        User GetLoggedInUser();
    }
}