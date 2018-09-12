using BasicAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DummyController : ControllerBase
    {
        private readonly IHubContext<ApiHub> _hubContext;
        private readonly DummyContext _context;
        private const string HUB_METHOD = "ReceiveMessage";
        public DummyController(DummyContext context, IHubContext<ApiHub> hubContext)
        {
            _hubContext = hubContext;
            _context = context;
            if (!_context.DummyItems.Any())
            {
                _context.DummyItems.AddAsync(new Dummy() { Id = 1, Name = "test", Description = "testing dummy" });
                _context.SaveChangesAsync();
            }
        }
        // GET api/dummy
        [HttpGet]
        public async Task<ActionResult<List<Dummy>>> Get([FromQuery] int page = 0, [FromQuery] int itemsPerPage = 10)
        {
            await _hubContext.
                Clients.
                All.
                SendAsync(HUB_METHOD,
                "Get",
                StatusCodes.Status200OK,
                $"Got results with parameters page: {page}, itemPerPage: {itemsPerPage}");
            return Ok(_context.
                DummyItems.
                OrderBy(d => d.Id).
                Skip(page * itemsPerPage).
                Take(itemsPerPage).
                ToList());
        }
        [HttpPost]
        public async Task<ActionResult<Dummy>> Create(Dummy item)
        {
            if (item == null)
                return this.ValidationProblem(new ValidationProblemDetails() { Detail = "No data provided" });
            var dummy = await _context.DummyItems.FindAsync(item.Id);
            if (dummy != null)
            {
                await _hubContext.
                        Clients.
                        All.
                        SendAsync(HUB_METHOD,
                        "Create",
                        StatusCodes.Status200OK,
                        $"Object already exists");
                return Ok(dummy);
            }
            await _context.DummyItems.AddAsync(item);
            await _context.SaveChangesAsync();
            await _hubContext.
                        Clients.
                        All.
                        SendAsync(HUB_METHOD,
                        "Create",
                        StatusCodes.Status201Created,
                        $"Object created");
            return CreatedAtRoute("GetDummyById", new { id = item.Id }, item);
        }
        // GET api/dummy/5
        [HttpGet("{id}", Name = "GetDummyById")]
        public async Task<ActionResult<Dummy>> GetById(int id)
        {
            var dummy = await _context.DummyItems.FindAsync(id);
            if (dummy == null)
            {
                await _hubContext.
                        Clients.
                        All.
                        SendAsync(HUB_METHOD,
                        "GetDummyById",
                        StatusCodes.Status404NotFound,
                        $"Object not found, id: {id}");
                return NotFound();
            }
            await _hubContext.
                        Clients.
                        All.
                        SendAsync(HUB_METHOD,
                        "GetDummyById",
                        StatusCodes.Status200OK,
                        $"Object was found, id: {id}");
            return Ok(dummy);
        }

        // PUT api/dummy/5?name=test&description=dd
        [HttpPut("{id}")]
        public async Task<ActionResult<Dummy>> Put(int id, [FromQuery] string name, [FromQuery] string description)
        {
            var dummy = await _context.DummyItems.FindAsync(id);
            if (dummy == null)
            {
                await _hubContext.
                        Clients.
                        All.
                        SendAsync(HUB_METHOD,
                        "Put",
                        StatusCodes.Status404NotFound,
                        $"Object not found, id: {id}");
                return NotFound(new { Id = id });
            }
            dummy.Name = name;
            dummy.Description = description;
            _context.DummyItems.Update(dummy);
            await _context.SaveChangesAsync();
            await _hubContext.
                        Clients.
                        All.
                        SendAsync(HUB_METHOD,
                        "Put",
                        StatusCodes.Status204NoContent,
                        $"Object was updated, id: {id}");
            return NoContent();
        }

        // DELETE api/dummy/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var dummy = await _context.DummyItems.FindAsync(id);
            if (dummy == null)
            {
                await _hubContext.
                        Clients.
                        All.
                        SendAsync(HUB_METHOD,
                        "Delete",
                        StatusCodes.Status404NotFound,
                        $"Object was not found, id: {id}");
                return NotFound();
            }
            _context.DummyItems.Remove(dummy);
            await _context.SaveChangesAsync();
            await _hubContext.
                        Clients.
                        All.
                        SendAsync(HUB_METHOD,
                        "Delete",
                        StatusCodes.Status204NoContent,
                        $"Object was deleted, id: {id}");
            return NoContent();
        }
    }
}
