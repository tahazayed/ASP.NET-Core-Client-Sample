using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.API.Entities;
using Products.API.Models;

namespace Products.API.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsDbContext _context;

        public ProductsController(ProductsDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet("{name}")]
        public async Task<IActionResult> GetProducts([FromRoute]string name = "")
        {
            if (!string.IsNullOrEmpty(name))
            {
                return Ok(await Task.Run(()=> _context.Products
                    .Where(p => p.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    .ToListAsync().GetAwaiter().GetResult()));
            }

            return Ok(await Task.Run(()=> _context.Products.ToListAsync().GetAwaiter().GetResult()));
        }


        // GET: api/Products/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute] long id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id).GetAwaiter().GetResult())
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] ProductForCreation productForCreation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = new Product
            {
                Name = productForCreation.Name,
                Price = productForCreation.Price,
                Category = productForCreation.Category
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        private async Task<bool> ProductExists(long id)
        {
            return await _context.Products.AnyAsync(e => e.Id == id);
        }
    }
}