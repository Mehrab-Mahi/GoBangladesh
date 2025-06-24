using System.Threading.Tasks;

namespace GoBangladesh.Application.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(string content, string email);
    }
}