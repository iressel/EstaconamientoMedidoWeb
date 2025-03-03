﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EstacionamientoMedido.Data;
using EstacionamientoMedido.Models;

namespace EstacionamientoMedido.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ClientsController _clientsController;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
            _clientsController = new ClientsController(context);
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var query = (from ticket in await _context.Tickets.ToListAsync()
                         join brand in await _context.Brands.ToListAsync() on Guid.Parse(ticket.BrandId) equals brand.Id
                         select new Ticket
                         {
                             Id = ticket.Id,
                             ClientDocumentNumber = ticket.ClientDocumentNumber,
                             ClientName = ticket.ClientName,
                             Patent = ticket.Patent,
                             VehicleModel = ticket.VehicleModel,
                             BrandName = brand.Name,
                             CheckIn = ticket.CheckIn,
                             CheckOut = ticket.CheckOut,
                             Date = ticket.Date,
                             Street = ticket.Street,
                             StreetHeight = ticket.StreetHeight,
                             ClientEmail = ticket.ClientEmail
                         }
                        ).ToList();
            return View(await _context.Tickets.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            var brands = await _context.Brands.ToListAsync();
            ViewBag.Brands = new SelectList(brands,"Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string clientDocumentNumber, 
            string clientName, string patent, string vehicleModel, string vehicleBrand, 
            string checkIn, string checkOut, string street, string streetHeight, string clientEmail)
        {
            var ticket = new Ticket();
            ticket.ClientDocumentNumber = clientDocumentNumber;
            ticket.ClientName = clientName;
            ticket.Patent = patent;
            ticket.VehicleModel = vehicleModel;
            ticket.BrandId = vehicleBrand;
            ticket.CheckIn = Convert.ToDateTime(checkIn).TimeOfDay;
            ticket.CheckOut = Convert.ToDateTime(checkOut).TimeOfDay;
            ticket.Street = street;
            ticket.StreetHeight = streetHeight;
            ticket.ClientEmail = clientEmail;
            ticket.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                ticket.Id = new Guid();
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                var client = new Client();
                client.Name = clientName;
                client.DocumentNumber = clientDocumentNumber;
                client.Email = clientEmail;

                await _clientsController.Create(client);

                dynamic result = new
                {
                    ticketId = ticket.Id
                };

                return Json(result);
            }

            return View(ticket);
        }

        // GET: Tickets/Comprobante
        public async Task<IActionResult> Comprobante(string ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(Guid.Parse(ticketId));
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ClientDocumentNumber,ClientName,Patent,VehicleModel,VehicleBrand,CheckIn,CheckOut,Date,Street,StreetHeight,ClientEmail")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(Guid id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
