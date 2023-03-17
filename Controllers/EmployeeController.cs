using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Repository;

namespace Task_2EF.Controllers
{
    [Authorize(Roles = "Visitor")]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IService<Employee> _service;
        private IMemoryCache _cache;
        public EmployeeController(IService<Employee> service)
        {
            _service = service;
        }

        // GET: api/Employee
        [HttpGet]
        public IActionResult Get()
        {

            IEnumerable<Employee> employees = _service.GetAll();
            return Ok(employees);
        }

        // GET: api/Employee/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(long id)
        {
            Employee employee = _service.Get(id);
            if (employee == null)
            {
                return NotFound("The Employee record couldn't be found.");
            }
            return Ok(employee);
        }

        // POST: api/Employee
        [HttpPost]
        public IActionResult Post([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest("Employee is null.");
            }
            _service.Add(employee);
            return CreatedAtRoute(
                  "Get",
                  new { Id = employee.EmployeeId },
                  employee);
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public IActionResult Put(long id, [FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest("Employee is null.");
            }
            Employee employeeToUpdate = _service.Get(id);
            if (employeeToUpdate == null)
            {
                return NotFound("The Employee record couldn't be found.");
            }
            _service.Update(employeeToUpdate, employee);
            return NoContent();
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            Employee employee = _service.Get(id);
            if (employee == null)
            {
                return NotFound("The Employee record couldn't be found.");
            }
            _service.Delete(employee);
            return NoContent();
        }
    }
}

