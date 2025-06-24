using System.Threading.Tasks;

namespace GoBangladesh.Application.Interfaces
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}