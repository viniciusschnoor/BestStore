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

        public IActionResult Index(int pageIndex, string? search, string? brand, string? category, string? sort)
        {
            //var products = context.Products.OrderByDescending(p => p.Id).ToList();

            // Create the Query (context.Products)
            IQueryable<Product> query = context.Products;

            // Search Functionality
            if (search != null && search.Length > 0)
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            // Filter Functionality
            if (brand != null && brand.Length > 0)
            {
                query = query.Where(p => p.Brand.Contains(brand));
            }

            if (category != null && category.Length > 0)
            {
                query = query.Where(p => p.Category.Contains(category));
            }

            // Sort Functionality
            if (sort == "price_asc")
            {
                query = query.OrderBy(p => p.Price);
            } else if (sort == "price_desc")
            {
                query = query.OrderByDescending(p => p.Price);
            } else
            {
                // Newest First
                query = query.OrderByDescending(p => p.Id);
            }

            // Update the Query to Sort Products (.OrderByDescending(p => p.Id))
            // query = query.OrderByDescending(p => p.Id);  -->  Sort Functionatility

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

            var storeSearchModel = new StoreSearchModel()
            {
                Search = search,
                Brand = brand,
                Category = category,
                Sort = sort
            };

            return View(storeSearchModel);
        }
    }
}
