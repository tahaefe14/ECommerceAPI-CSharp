using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly AppDbContext _context;
        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder()
        {
            var cartItems = await _context.CartItems.Include(c => c.Product).ToListAsync();
            if (!cartItems.Any())
            {
                return BadRequest("Sepet boş");
            }

            var order = new Order
            {
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId=c.ProductId,
                    Quantity=c.Quantity,
                    UnitPrice=c.Product!.Price,
                }).ToList(),
                TotalPrice=cartItems.Sum(c=>c.Product!.Price *c.Quantity)
            };

            //stok güncelleme
            foreach (var item in cartItems)
            {
                item.Product!.Stock -= item.Quantity;
            }
            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders.Include(o => o.Items).ThenInclude(i => i.Product).ToListAsync();
            return Ok(orders);
        }
    }
}
