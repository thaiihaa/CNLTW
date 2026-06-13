using Microsoft.AspNetCore.Mvc;
using APIBaiTap.Models;

namespace APIBaiTap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // In-memory store for demo purposes
        private static readonly List<Product> _products = new();
        private static int _nextId = 1;

        [HttpPost]
        public IActionResult Create([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                // Return list of validation errors as JSON
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => new
                    {
                        field = kvp.Key,
                        errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    })
                    .ToArray();

                return BadRequest(errors);
            }

            product.Id = _nextId++;
            _products.Add(product);

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            // Validate id: must be positive integer
            if (id <= 0)
            {
                var errors = new[]
                {
                    new {
                        field = "id",
                        errors = new[] { "Id must be a positive integer" }
                    }
                };

                return BadRequest(errors);
            }

            var p = _products.FirstOrDefault(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_products);
        }
    }
}
