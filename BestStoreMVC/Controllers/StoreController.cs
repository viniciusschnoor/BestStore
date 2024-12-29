using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    public class StoreController : Controller
    {
        private ApplicationDbContext context;
        private readonly int pageSize = 12;

        public StoreController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            //var products = context.Products.OrderByDescending(p => p.Id).ToList();

            // Create the Query (context.Products)
            IQueryable<Product> query = context.Products;

            // Update the Query to Sort Products (.OrderByDescending(p => p.Id))
            query = query.OrderByDescending(p => p.Id);

            // Implementation of Pagination Functionality
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            // Read products from Requested Page
            decimal count = query.Count(); // Total Items
            int totalPages = (int)Math.Ceiling(count / pageSize); // 30/12 = 2.5 ~ 3;
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            // List products based on Query (.ToList())
            var products = query.ToList();

            ViewBag.Products = products;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }
    }
}
