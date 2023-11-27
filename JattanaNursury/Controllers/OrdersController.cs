﻿using AutoMapper;
using JattanaNursury.Data;
using JattanaNursury.Models;
using JattanaNursury.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;

namespace JattanaNursury.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() 
        {
            var orders = _context.Orders;
            List<OrderIndexViewModel> result = new();
            foreach (var order in orders)
            {
                result.Add(new OrderIndexViewModel { OrderNumber = order.OrderNumber, OrderId = order.Id, OrderDate = order.OrderDate.ToString("o"), BillPrice = order.BillPrice, Price = order.Price, Discount = order.Discount,Employee = order.EmployeeId });
            }
            return View(result);
        }

        public IActionResult Create()
        {
            return View();
        }

        public class ProductModel
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal Quantity { get; set; }
            public decimal TotalPrice { get; set; }
        }

        [HttpGet]
        public async Task<List<ProductModel>> GetProductsByNameAsync(string search = "")
        {
            List<ProductModel> products = new();
            
            try
            {
                var pList = await _context.Products.Where(a => a.Name.ToLower().Contains(search.ToLower())).ToListAsync();
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<Product, ProductModel>()));
                products = mapper.Map<List<Product>, List<ProductModel>>(pList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return products;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSaleOrderAsync([FromBody] OrderViewModel saleOrder)
        {

            if (ModelState.IsValid)
            {
                if (saleOrder == null) return View(nameof(Create));

                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<OrderViewModel, Customer>()));
                var customer = mapper.Map<OrderViewModel, Customer>(saleOrder);
                customer.CreatedDate = DateTime.UtcNow;
                _context.Customers.Add(customer);

                var order = new Order { CustomerId = customer.Id, OrderDate = DateTime.UtcNow, Discount = saleOrder.Discount, EmployeeId = saleOrder.Employee };

                decimal totalPrice = 0;
                foreach (var item in saleOrder.Products)
                {
                    var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);

                    if (product == null || item.Quantity < 1 || product.Quantity < item.Quantity)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    var productOrder = new ProductOrder { ProductId = product.Id, Quantity = item.Quantity, TotalPrice = product.UnitPrice * item.Quantity };
                    totalPrice += productOrder.TotalPrice;
                    order.ProductOrders?.Add(productOrder);
                    product.Quantity -= item.Quantity;
                }

                order.Price = totalPrice;

                var discountPercetage = (saleOrder.Discount / totalPrice) * 100;

                if (discountPercetage > 20) 
                {
                    return RedirectToAction(nameof(Index));
                }

                order.BillPrice = order.Price  - saleOrder.Discount;
                var totalOrders = _context.Orders.Count();
                order.OrderNumber = (totalOrders + 1001).ToString();
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
