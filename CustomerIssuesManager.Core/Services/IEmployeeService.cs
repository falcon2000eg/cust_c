using CustomerIssuesManager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(int id);
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
        Task<bool> EmployeeExistsAsync(string name);
        Task<bool> PerformanceNumberExistsAsync(string performanceNumber);
        Task<Employee> GetEmployeeByPerformanceNumberAsync(string performanceNumber);
        Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
        Task<int> GetEmployeeCaseCountAsync(int employeeId);
        Task<IEnumerable<Employee>> GetTopEmployeesAsync(int count = 5);
    }
}
