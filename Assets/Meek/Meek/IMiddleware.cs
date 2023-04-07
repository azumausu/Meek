using System.Threading.Tasks;

namespace Meek
{
    public interface IMiddleware
    {
        ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next);
    }
}