using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassTypesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClassTypesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("get-all-classtypes")]
        public async Task<ActionResult<ResponseModel>> GetAll()
        {
            ResponseModel response = new ResponseModel();
            try
            {

                var classes = await _context.ClassTypes
                    .Include(c => c.Sessions)
                    .ToListAsync();
                response.Message = "Data Retrieved";
                response.Model = _mapper.Map<List<ClassTypeResponseDto>>(classes);
                response.Status = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                response.Model = null;
                response.Status = false;
                return response;
            }
        }

        [HttpPost("get-single-classtype")]
        public async Task<IActionResult> Get([FromBody] int id)
        {
            ResponseModel response = new ResponseModel();
            try
            {

                var classType = await _context.ClassTypes
                .Include(c => c.Sessions)
                .FirstOrDefaultAsync(c => c.Id == id);

                if (classType == null) return NotFound();
                response.Model = _mapper.Map<ClassTypeResponseDto>(classType);
                response.Message = "Data Retrieved";
                response.Status = true;
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                response.Model = null;
                response.Status = false;
                return new BadRequestObjectResult(response);
            }
        }

        [HttpPost("create-class-type")]
        public async Task<ActionResult<ClassTypeResponseDto>> Create([FromBody] CreateClassTypeDto dto)
        {
            ResponseModel response = new ResponseModel();
            try
            {

                var classType = _mapper.Map<ClassType>(dto);
                _context.ClassTypes.Add(classType);
                await _context.SaveChangesAsync();

                //return CreatedAtAction(nameof(Get), new { id = classType.Id }, _mapper.Map<ClassTypeResponseDto>(classType));
                response.Model = _mapper.Map<ClassTypeResponseDto>(classType);
                response.Message = "Data Retrieved";
                response.Status = true;
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                response.Model = null;
                response.Status = false;
                return new BadRequestObjectResult(response);
            }
        }

        [HttpPost("update-class-type")]
        public async Task<IActionResult> Update([FromBody] UpdateClassTypeDto dto)
        {
            ResponseModel response = new ResponseModel();

            try
            {
                var classType = await _context.ClassTypes.FindAsync(dto.classid);
                if (classType == null)
                {
                    throw new Exception("Not Found");
                };

                _mapper.Map(dto, classType);
                await _context.SaveChangesAsync();
                response.Model = classType;
                response.Message = "Data Retrieved";
                response.Status = true;
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
                response.Model = null;
                response.Status = false;
                return new BadRequestObjectResult(response);
            }
        }

        [HttpPost("delete-class-id")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            ResponseModel response = new ResponseModel();

            try
            {
                var classType = await _context.ClassTypes.FindAsync(id);
                if (classType == null)
                {
                    throw new Exception("Not Found");
                };
                _context.ClassTypes.Remove(classType);
                await _context.SaveChangesAsync();
                response.Message = "Delete Success";
                response.Status = true;
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
                response.Model = null;
                response.Status = false;
                return new BadRequestObjectResult(response);
            }
        }
    }
}
