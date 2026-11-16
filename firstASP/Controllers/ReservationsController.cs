
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using firstASP.Data;
using firstASP.Models;

namespace firstASP.Controllers;

public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReservationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Reservation form
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Reservation
        {
            StartDateTime = DateTime.Now.AddHours(1),
            EndDateTime = DateTime.Now.AddHours(2)
        });
    }

    // Submit reservation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reservation reservation)
    {
        if (reservation.StartDateTime <= DateTime.Now)
        {
            ModelState.AddModelError(
                "StartDateTime",
                "Please select a future date and time.");
        }

        if (reservation.EndDateTime <= reservation.StartDateTime)
        {
            ModelState.AddModelError(
                "EndDateTime",
                "The ending time must be later than the starting time.");
        }

        if (ModelState.IsValid)
        {
            bool hasConflict = await _context.Reservations.AnyAsync(r =>
                r.Status != "Rejected" &&
                r.StartDateTime < reservation.EndDateTime &&
                r.EndDateTime > reservation.StartDateTime);

            if (hasConflict)
            {
                ModelState.AddModelError(
                    "",
                    "This time slot is already reserved. Please choose another schedule.");
            }
            else
            {
                reservation.Status = "Pending";
                reservation.CreatedAt = DateTime.Now;

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                return RedirectToAction(
                    nameof(Confirmation),
                    new { id = reservation.Id });
            }
        }

        return View(reservation);
    }

    // View all reservations
    public async Task<IActionResult> Index()
    {
        var reservations = await _context.Reservations
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reservations);
    }

    // Admin approval dashboard - open to everyone
    public async Task<IActionResult> Admin()
    {
        var reservations = await _context.Reservations
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reservations);
    }

    // Approve reservation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
        {
            return NotFound();
        }

        if (reservation.Status != "Pending")
        {
            return RedirectToAction(nameof(Admin));
        }

        bool hasConflict = await _context.Reservations.AnyAsync(r =>
            r.Id != id &&
            r.Status == "Approved" &&
            r.StartDateTime < reservation.EndDateTime &&
            r.EndDateTime > reservation.StartDateTime);

        if (hasConflict)
        {
            TempData["AdminMessage"] =
                "This reservation conflicts with an already approved reservation.";
        }
        else
        {
            reservation.Status = "Approved";
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                "Reservation successfully approved.";
        }

        return RedirectToAction(nameof(Admin));
    }

    // Reject reservation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
        {
            return NotFound();
        }

        if (reservation.Status == "Pending")
        {
            reservation.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                "Reservation successfully rejected.";
        }

        return RedirectToAction(nameof(Admin));
    }

    // Confirmation page
    public async Task<IActionResult> Confirmation(int id)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }
}
