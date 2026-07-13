using CoreAuthAndAuthUsingJWTToken.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedLIbrary.DTOs;
using SharedLIbrary.Models;

namespace CoreAuthAndAuthUsingJWTToken.Controllers{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase{
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public EmployeesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;  _env = env;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _context.Employees.Include(e => e.Experiences).ToListAsync();
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int id){
            var employee = await _context.Employees
                .Include(e => e.Experiences)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null){
                return NotFound(new { message = $"Employee with ID {id} not found." });
            }
            return Ok(employee);
        }       
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostEmployee([FromForm] Common objCommon)
        {
            if (objCommon == null) return BadRequest("Invalid data.");
            string fileName = "";
            string folderPath = Path.Combine(_env.WebRootPath, "images");
            if (objCommon.ImageFile != null && objCommon.ImageFile.Length > 0)
            {
                if (!Directory.Exists(folderPath)){
                    Directory.CreateDirectory(folderPath);
                }
                fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(objCommon.ImageFile.FileName);
                string filePath = Path.Combine(folderPath, fileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {await objCommon.ImageFile.CopyToAsync(stream); }
            }
            Employee empObj = new Employee{
                EmployeeName = objCommon.EmployeeName,
                IsActive = objCommon.IsActive,
                JoinDate = objCommon.JoinDate,
                ImageUrl = "/images/" + fileName
            };
            if (!string.IsNullOrEmpty(objCommon.Experiences)){
                try{
                    var expList = JsonConvert.DeserializeObject<List<Experience>>(objCommon.Experiences);
                    if (expList != null) empObj.Experiences = expList;
                }
                catch (JsonException ex){
                    return BadRequest("Invalid Experience data format.");
                }
            }
            _context.Employees.Add(empObj);
            await _context.SaveChangesAsync();
            return Ok(empObj);
         
        }
        [HttpPost("Upload/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> Upload(int id, IFormFile file)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound("Employee not found.");
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext)) return BadRequest("Invalid file type.");
            if (file.Length > 2 * 1024 * 1024) return BadRequest("File size exceeds 2MB.");
            string fileName = Guid.NewGuid().ToString() + ext;
            string folderPath = Path.Combine(_env.WebRootPath, "Pictures");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string savePath = Path.Combine(folderPath, fileName);
            try{
                using (var fs = new FileStream(savePath, FileMode.Create))
                { await file.CopyToAsync(fs);}
                if (!string.IsNullOrEmpty(employee.ImageUrl))
                {
                    var oldPath = Path.Combine(folderPath, employee.ImageUrl);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                employee.ImageUrl = fileName; 
                _context.Entry(employee).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { fileName, message = "Image uploaded successfully" });
            }
            catch (Exception ex){
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutEmployee(int id, [FromForm] Common objCommon)
        {
          if (id != objCommon.EmployeeId) return BadRequest("ID Mismatch");
            var empObj = await _context.Employees
                .Include(e => e.Experiences)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (empObj == null) return NotFound("Employee not found.");
            if (objCommon.ImageFile != null && objCommon.ImageFile.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "images");         
                if (!string.IsNullOrEmpty(empObj.ImageUrl))
                {
                   var oldFileName = Path.GetFileName(empObj.ImageUrl);
                    var oldPath = Path.Combine(folderPath, oldFileName);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
                string newFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(objCommon.ImageFile.FileName);
                string filePath = Path.Combine(folderPath, newFileName);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await objCommon.ImageFile.CopyToAsync(stream);
                }
                empObj.ImageUrl = "/images/" + newFileName;
            }
            empObj.EmployeeName = objCommon.EmployeeName;
            empObj.IsActive = objCommon.IsActive;
            empObj.JoinDate = objCommon.JoinDate;
            _context.Experiences.RemoveRange(empObj.Experiences);
            if (!string.IsNullOrEmpty(objCommon.Experiences))
            {
                try{
                    var expList = JsonConvert.DeserializeObject<List<Experience>>(objCommon.Experiences);
                    if (expList != null){
                        foreach (var exp in expList){
                            exp.EmployeeId = id;
                            empObj.Experiences.Add(exp);
                        }
                    }
                }
                catch (JsonException){
                    return BadRequest("Invalid Experience data format.");
                }
            }
            try{
                _context.Entry(empObj).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException){
                if (!_context.Employees.Any(e => e.EmployeeId == id)) return NotFound();
                throw;
            }
            return Ok(empObj);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(int id){
            var employee = await _context.Employees.Include(e => e.Experiences).FirstOrDefaultAsync(x => x.EmployeeId == id);
            if (employee == null) return NotFound();
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
