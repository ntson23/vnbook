﻿using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using WebBook.Data;
using WebBook.ViewModels;
using X.PagedList;

namespace WebBook.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("product")]
        public IActionResult Index(int? page, string searchString, string currentFilter)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            int pageSize = 12;
			int pageNumber = (page ?? 1);

			var products = _context.Products?.ToList();
            var listVM = new List<ProductVM>();
            foreach (var item in products)
            {
                ProductVM vm = new()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    Discount = item.Discount,
                    Quantity = item.Quantity,
                    Avatar = _context.ProductImages.Where(x => x.ProductId == item.Id).ToList().FirstOrDefault(x => x.IsAvatar)?.ImageName,
                    CategorySlug = _context.Categories?.FirstOrDefault(x => x.Id == item.CategoryId)?.Slug,
                    Slug = item.Slug,
                    CategoryName = _context.Categories?.FirstOrDefault(x=>x.Id == item.CategoryId)?.Name,
                    Author = item.Author,
                    SupplierName = _context.Suppliers?.FirstOrDefault(x=>x.Id == item.SupplierId)?.Name

                };
                listVM.Add(vm);
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                listVM = listVM.Where(x => x.Name.ToLower().Contains(searchString)
                || x.Author.ToLower().Contains(searchString) 
                || x.CategoryName.ToLower().Contains(searchString)
                || x.SupplierName.ToLower().Contains(searchString)
                ).ToList();
            }

            return View(listVM.ToPagedList(pageNumber, pageSize));
        }

        [Route("product/{slug}")]
        public IActionResult ProductCategory(string slug, int? page)
        {
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            var categoryId = _context.Categories?.FirstOrDefault(x => x.Slug == slug)?.Id;
            if (categoryId != null)
            {
                ViewBag.categoryId = categoryId;
                ViewBag.categoryName = _context.Categories?.Find(categoryId)?.Name;
                ViewBag.categorySlug = slug;
                var products = _context.Products?.Where(x => x.CategoryId == categoryId).ToList();
                var listVM = new List<ProductVM>();
                foreach (var item in products)
                {
                    ProductVM vm = new()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = item.Price,
                        Discount = item.Discount,
                        //PriceSale = item.PriceSale,
                        Avatar = _context.ProductImages.Where(x => x.ProductId == item.Id).ToList().FirstOrDefault(x => x.IsAvatar)?.ImageName,
                        CategorySlug = _context.Categories?.FirstOrDefault(x => x.Id == item.CategoryId)?.Slug,
                        Slug = item.Slug

                    };
                    listVM.Add(vm);
                }
                return View(listVM.ToPagedList(pageNumber, pageSize));
            }
            return NotFound();
        }

        [Route("product/{slug}/{id}")]
        public IActionResult Detail(int id)
        {
            var product = _context.Products?.FirstOrDefault(x => x.Id == id);
           
            if (product != null)
            {
                ProductVM vm = new()
                {
                    Id = product.Id,
                    Name = product.Name,
                    Slug = product.Slug,
                    Description = product.Description,
                    Price = product.Price,
                    Discount = product.Discount,
                    Quantity = product.Quantity,
                    Avatar = _context.ProductImages.FirstOrDefault(x => x.ProductId == product.Id && x.IsAvatar)?.ImageName,
                    Author = product.Author,
                    CategoryId = product.CategoryId,
                    SupplierId = product.SupplierId,
                    CategoryName = _context.Categories?.FirstOrDefault(x => x.Id == product.CategoryId)?.Name,
                    SupplierName = _context.Suppliers?.FirstOrDefault(x => x.Id == product.SupplierId)?.Name
                };

                // Lấy danh sách sản phẩm cùng loại (cùng CategoryId)
                var relatedProducts = _context.Products
                                        .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                                        .ToList();
                var relatedProductsVM = relatedProducts.Select(rp => new ProductVM
                {
                    Id = rp.Id,
                    Name = rp.Name,
                    Slug = rp.Slug,
                    Description = rp.Description,
                    Price = rp.Price,
                    Discount = rp.Discount,
                    Quantity = rp.Quantity,
                    Avatar = _context.ProductImages.FirstOrDefault(x => x.ProductId == rp.Id && x.IsAvatar)?.ImageName,
                    Author = rp.Author,
                    CategoryId = rp.CategoryId,
                    SupplierId = rp.SupplierId,
                    CategoryName = _context.Categories?.FirstOrDefault(x => x.Id == rp.CategoryId)?.Name,
                    SupplierName = _context.Suppliers?.FirstOrDefault(x => x.Id == rp.SupplierId)?.Name
                }).ToList();

                ViewBag.RelatedProducts = relatedProductsVM;

                var reviews = _context.Reviews.Where(x => x.ProductId == product.Id).ToList();
                int rating = 0;
                foreach(var item in reviews)
                {
                    rating += item.Rating + 1;
                }

                if (reviews.Count > 0)
                {
                    ViewBag.rating = Math.Round(((float)rating / reviews.Count));
                }
                else
                {
                    ViewBag.rating = 0;
                }
               
                ViewBag.categoryName = vm.CategoryName;
                ViewBag.categorySlug = _context.Categories?.FirstOrDefault(x => x.Id == product.CategoryId)?.Slug;
                return View(vm);
            }
            return NotFound();
        }

       
    }
}
