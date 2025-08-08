using CustomerIssuesManager.Core.Data;
using CustomerIssuesManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> LoginByPerformanceNumberAsync(string performanceNumber)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.IsActive && e.PerformanceNumber == performanceNumber);
    }
}
