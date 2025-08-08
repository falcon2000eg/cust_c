using CustomerIssuesManager.Core.Models;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services;

public interface IAuthService
{
    Task<Employee?> LoginByPerformanceNumberAsync(string performanceNumber);
}
