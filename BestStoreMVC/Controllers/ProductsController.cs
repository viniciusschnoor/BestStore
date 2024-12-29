using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace BestStoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly int pageSize = 5;

        //public string NameFile(ProductDto productDto)
        //{
        //    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        //    newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

        //    string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
        //    using (FileStream stream = System.IO.File.Create(imageFullPath))
        //    {
        //        productDto.ImageFile.CopyTo(stream);
        //    }
        //    return newFileName;
        //}

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        // INDEX
        public IActionResult Index(int pageIndex, string? search, string? column, string? orderBy)
        {
            IQueryable<Product> query = context.Products;

            // Funcionalidade de Pesquisa
            if (search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }

            // Funcionalidade de Ordenação
            string[] validColumns = { "Id", "Name", "Brand", "Category", "Price", "CreatedAt" };
            string[] validOrderBy = { "desc", "asc" };

            if (!validColumns.Contains(column))
            {
                column = "Id";
            }

            if (!validOrderBy.Contains(orderBy))
            {
                orderBy = "desc";
            }

            #region MySolution
            // Dicionário que mapeia o nome da coluna para a expressão de ordenação.
            Dictionary<string, Expression<Func<Product, object>>> ordenacaoDicionario = new Dictionary<string, Expression<Func<Product, object>>>
            {
                { "Name", p => p.Name },
                { "Brand", p => p.Brand },
                { "Category", p => p.Category },
                { "Price", p => p.Price },
                { "CreatedAt", p => p.CreatedAt },
                { "Id", p => p.Id} // Ordenação padrão por Id
            };

            // Tenta obter a expressão de ordenação com base no nome da coluna.
            if (ordenacaoDicionario.TryGetValue(column, out Expression<Func<Product, object>> expressaoOrdenacao))
            {
                // Aplica a ordenação correta (ascendente ou descendente).
                query = orderBy == "asc"
                    ? query.OrderBy(expressaoOrdenacao)
                    : query.OrderByDescending(expressaoOrdenacao);
            }
            else
            {
                // Tratamento para coluna inválida (opcional, mas recomendado).
                // Pode lançar uma exceção ou usar um log.
                // throw new ArgumentException($"Coluna para ordenação inválida: {column}");
                // Ou usar um valor padrão:
                query = orderBy == "asc" ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
                Console.WriteLine($"Aviso: Coluna '{column}' não encontrada. Ordenando por Id.");
            }
            #endregion

            #region CourseSolution
            //if (column == "Name")
            //{
            //    if (orderBy == "asc")
            //    {
            //        query = query.OrderBy(p => p.Name);
            //    }
            //    else
            //    {
            //        query = query.OrderByDescending(p => p.Name);
            //    }
            //}
            //else if (column == "Brand")
            //{
            //    if (orderBy == "asc")
            //    {
            //        query = query.OrderBy(p => p.Brand);
            //    }
            //    else
            //    {
            //        query = query.OrderByDescending(p => p.Brand);
            //    }
            //}
            //else if (column == "Category")
            //{
            //    if (orderBy == "asc")
            //    {
            //        query = query.OrderBy(p => p.Category);
            //    }
            //    else
            //    {
            //        query = query.OrderByDescending(p => p.Category);
            //    }
            //} else if (column == "Price")
            //{
            //    if (orderBy == "asc")
            //    {
            //        query = query.OrderBy(p => p.Price);
            //    }
            //    else
            //    {
            //        query = query.OrderByDescending(p => p.Price);
            //    }
            //} else if (column == "CreatedAt")
            //{
            //    if (orderBy == "asc")
            //    {
            //        query = query.OrderBy(p => p.CreatedAt);
            //    }
            //    else
            //    {
            //        query = query.OrderByDescending(p => p.CreatedAt);
            //    }
            //} else
            //{
            //    if (orderBy == "asc")
            //    {
            //        query = query.OrderBy(p => p.Id);
            //    }
            //    else
            //    {
            //        query = query.OrderByDescending(p => p.Id);
            //    }
            //}
            #endregion

            // query = query.OrderByDescending(p => p.Id);

            // Funcionalidade de Pesquisa
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var products = query.ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;

            ViewData["Search"] = search ?? "";

            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;

            return View(products);
        }

        #region CRUD
        // CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            // Save the Image File
            //string newFileName = NameFile(productDto);
            DateTime CreateDateTime = DateTime.Now;
            string newFileName = CreateDateTime.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
            using (FileStream stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // Save the new product in the database
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = CreateDateTime
            };

            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

        // READ

        // UPDATE
        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            // Create productDto from product
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt;

            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt;

                return View(productDto);
            }

            // Update the Image File if we have a new
            string newFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

                string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                // Delete the old image
                string oldImageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }

        // DELETE
        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            string imageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);

            context.Products.Remove(product);
            context.SaveChanges(true);

            return RedirectToAction("Index", "Products");
        }
        #endregion
    }
}
