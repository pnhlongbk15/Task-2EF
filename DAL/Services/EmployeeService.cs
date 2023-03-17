using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Repository;

namespace Task_2EF.DAL.DataManager
{
    public class EmployeeService : IService<Employee>
    {
        private readonly ApplicationContext _context;
        public EmployeeService(ApplicationContext context)
        {
            _context = context;
        }
        public void Add(Employee entity)
        {
            try
            {
                _context.Employees.Add(entity);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(Employee entity)
        {
            try
            {
                _context.Employees.Remove(entity);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Employee Get(long id)
        {
            return _context.Employees.FirstOrDefault(e => e.EmployeeId == id);
        }

        public IEnumerable<Employee> GetAll()
        {
            return _context.Employees.ToList();
        }

        public void Update(Employee employee, Employee entity)
        {
            try
            {
                employee.FirstName = entity.FirstName;
                employee.LastName = entity.LastName;
                employee.Email = entity.Email;
                employee.DateOfBirth = entity.DateOfBirth;
                employee.PhoneNumber = entity.PhoneNumber;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
