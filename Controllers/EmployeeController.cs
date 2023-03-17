using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Models;
using Task_2EF.DAL.Repository;

namespace Task_2EF.Controllers
{
    //[Authorize(Roles = "Visitor")]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IService<Employee> _service;
        private IMemoryCache _cache;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public MemoryCacheEntryOptions options { get; set; }

        public EmployeeController(IService<Employee> service, IMemoryCache cache, IMapper mapper)
        {
            _cache = cache;
            _service = service;
            _mapper = mapper;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<IActionResult> GetAllEmployee()
        {
            if (_cache.TryGetValue<IEnumerable<EmployeeModel>>("GetAllEmployee", out var aEmployee))
            {
                return Ok(aEmployee);
            }
            else
            {
                try
                {
                    await semaphore.WaitAsync();
                    if (_cache.TryGetValue("GetAllEmployee", out aEmployee))
                    {
                        return Ok(aEmployee);
                    }
                    aEmployee = _mapper.Map<IEnumerable<EmployeeModel>>(_service.GetAll());

                    _cache.Set("GetAllEmployee", aEmployee, options);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            return Ok(aEmployee);
        }

        // GET: api/Employee/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult GetEmployeeById(long id)
        {
            EmployeeModel mEmployee = _mapper.Map<EmployeeModel>(_service.Get(id));
            if (mEmployee == null)
            {
                return NotFound("The Employee record couldn't be found.");
            }
            return Ok(mEmployee);
        }

        // POST: api/Employee
        [HttpPost]
        public IActionResult AddOneNew([FromBody] EmployeeModel mEmployee)
        {
            if (mEmployee == null)
            {
                return BadRequest("Employee is null.");
            }

            try
            {
                Employee eEmployee = _mapper.Map<Employee>(mEmployee);
                _service.Add(eEmployee);
                // remove cache employee
                _cache.Remove("GetAllEmployee");

                //return CreatedAtRoute("Get", new { Id = eEmployee.EmployeeId }, mEmployee);
                return Ok("Add successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public IActionResult UpdateInfoForOne(long id, [FromBody] EmployeeModel mEmployee)
        {
            if (mEmployee == null)
            {
                return BadRequest("Employee is null.");
            }
            try
            {
                Employee employeeToUpdate = _service.Get(id);
                if (employeeToUpdate == null)
                {
                    return NotFound("The Employee record couldn't be found.");
                }
                Employee aEmployee = _mapper.Map<Employee>(mEmployee);
                _service.Update(employeeToUpdate, aEmployee);

                _cache.Remove("GetAllEmployee");

                return Ok("Success.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public IActionResult RemoveOne(long id)
        {
            try
            {
                Employee employee = _service.Get(id);
                if (employee == null)
                {
                    return NotFound("The Employee record couldn't be found.");
                }
                _service.Delete(employee);
                _cache.Remove("GetAllEmployee");
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
