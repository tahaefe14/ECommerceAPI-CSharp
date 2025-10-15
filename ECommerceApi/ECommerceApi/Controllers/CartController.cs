using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _context.CartItems.Include(c => c.Product).ToListAsync();
            return Ok(cart);
        }

        [HttpPost("productId")]
        public async Task<IActionResult> AddToCart(int productId, [FromQuery] int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product==null||product.Stock<quantity)
            {
                return BadRequest("Ürün bulunamadı veya stok yetersiz.");
            }

            var existing = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId);
            if (existing !=null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem { ProductId = productId, Quantity = quantity });

            }

            await _context.SaveChangesAsync();
            return Ok("sepete eklendi");
        }

        [HttpDelete("productId")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId);
            if (item==null)
            {
                return NotFound("Sepet ögesi bulunamadı");
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok("Sepetten Çıkarıldı");
        }
    }
}
