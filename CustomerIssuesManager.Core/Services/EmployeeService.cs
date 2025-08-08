using CustomerIssuesManager.Core.Data;
using CustomerIssuesManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Entry(employee).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                // Soft delete - mark as inactive instead of hard delete
                employee.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<bool> EmployeeExistsAsync(string name)
        {
            return await _context.Employees
                .AnyAsync(e => e.Name == name && e.IsActive);
        }

        public async Task<bool> PerformanceNumberExistsAsync(string performanceNumber)
        {
            return await _context.Employees
                .AnyAsync(e => e.PerformanceNumber == performanceNumber && e.IsActive);
        }

        public async Task<Employee> GetEmployeeByPerformanceNumberAsync(string performanceNumber)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.PerformanceNumber == performanceNumber && e.IsActive);
        }

        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
        {
            return await _context.Employees
                .Where(e => e.IsActive && 
                           (e.Name.Contains(searchTerm) || 
                            e.Position.Contains(searchTerm) || 
                            e.PerformanceNumber.Contains(searchTerm)))
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<int> GetEmployeeCaseCountAsync(int employeeId)
        {
            return await _context.Cases
                .CountAsync(c => c.CreatedById == employeeId);
        }

        public async Task<IEnumerable<Employee>> GetTopEmployeesAsync(int count = 5)
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    Employee = e,
                    CaseCount = _context.Cases.Count(c => c.CreatedById == e.Id)
                })
                .OrderByDescending(x => x.CaseCount)
                .Take(count)
                .Select(x => x.Employee)
                .ToListAsync();
        }
    }
}
