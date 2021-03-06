﻿using App.Model;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class SalesController : Controller
    {
        private MainDBContext db = new MainDBContext();

        // GET: Sales
        public ActionResult Index() 
        {
            var sales = db.Sales.Include(s => s.Product).OrderByDescending(s => s.SaleDate);
            return View(sales.ToList());
        }

        // GET: Sales/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sales.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            return View(sale);
        }

        // GET: Sales/Add
        public ActionResult Add()
        {
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name");
            return View();
        }

        // POST: Sales/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Include = "Id,ProductId,Quantity")] Sale sale)
        {
            if (ModelState.IsValid)
            {
                Inventory productInventory = db.Inventory.Where(p => p.ProductId == sale.ProductId).FirstOrDefault<Inventory>();
                if (productInventory.Quantity >= sale.Quantity)
                {
                    // get amount
                    sale.Amount = sale.Quantity * db.Products.Find(sale.ProductId).Price;
                    // add sale
                    sale.SaleDate = DateTime.Now;
                    db.Sales.Add(sale);
                    // reduce stock
                    productInventory.Quantity -= sale.Quantity;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                // Error- not enough stock
                // else {}
            }

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", sale.ProductId);
            return View(sale);
        }

        // GET: Sales/Edit/id
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sales.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", sale.ProductId);
            return View(sale);
        }

        // POST: Sales/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ProductId,Quantity")] Sale sale)
        {
            if (ModelState.IsValid)
            {
                Sale entry = db.Sales.Find(sale.Id);
                decimal rate = entry.Amount / entry.Quantity;
                entry.Amount = sale.Quantity * rate;
                entry.Quantity = sale.Quantity;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", sale.ProductId);
            return View(sale);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
