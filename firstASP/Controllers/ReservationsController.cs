
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using firstASP.Data;
using firstASP.Models;

namespace firstASP.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReservationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // STUDENT: Display reservation form
    [Authorize(Roles = "Student")]
    [HttpGet]
    public IActionResult Create()
    {
        var reservation = new Reservation
        {
            StartDateTime = DateTime.Now.AddHours(1),
            EndDateTime = DateTime.Now.AddHours(2),
            BenildeEmail = User.Identity!.Name!
        };

        return View(reservation);
    }

    // STUDENT: Submit reservation
    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reservation reservation)
    {
        string email = User.Identity!.Name!;

        if (string.IsNullOrWhiteSpace(reservation.BenildeEmail) ||
            !string.Equals(
                reservation.BenildeEmail.Trim(),
                email,
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                "BenildeEmail",
                "Please enter the same Benilde email used to log in.");
        }

        reservation.BenildeEmail = email;

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

        if (!ModelState.IsValid)
        {
            return View(reservation);
        }

        bool hasConflict = await _context.Reservations.AnyAsync(r =>
            r.Status != "Rejected" &&
            r.StartDateTime < reservation.EndDateTime &&
            r.EndDateTime > reservation.StartDateTime);

        if (hasConflict)
        {
            ModelState.AddModelError(
                "",
                "This time slot is already reserved. Please choose another schedule.");

            return View(reservation);
        }

        reservation.Status = "Pending";
        reservation.CreatedAt = DateTime.Now;
        reservation.IsNotificationRead = true;
        reservation.StatusUpdatedAt = null;

        _context.Reservations.Add(reservation);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Confirmation),
            new { id = reservation.Id });
    }

    // STUDENT: View own reservations
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Index()
    {
        string email = User.Identity!.Name!;

        var reservations = await _context.Reservations
            .Where(r => r.BenildeEmail == email)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reservations);
    }

    // ADMIN: View all reservations
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var reservations = await _context.Reservations
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reservations);
    }

    // STUDENT AND ADMIN: View court schedule
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> Schedule(DateTime? date)
    {
        var selectedDate = (date ?? DateTime.Today).Date;

        int daysFromMonday =
            ((int)selectedDate.DayOfWeek + 6) % 7;

        var weekStart = selectedDate.AddDays(-daysFromMonday);
        var weekEnd = weekStart.AddDays(7);

        var reservations = await _context.Reservations
            .Where(r =>
                r.Status != "Rejected" &&
                r.StartDateTime < weekEnd &&
                r.EndDateTime > weekStart)
            .OrderBy(r => r.StartDateTime)
            .ToListAsync();

        ViewBag.WeekStart = weekStart;
        ViewBag.PreviousWeek = weekStart.AddDays(-7);
        ViewBag.NextWeek = weekStart.AddDays(7);

        return View(reservations);
    }

    // STUDENT: View notifications
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Notifications()
    {
        string email = User.Identity!.Name!;

        var notifications = await _context.Reservations
            .Where(r =>
                r.BenildeEmail == email &&
                r.StatusUpdatedAt != null)
            .OrderByDescending(r => r.StatusUpdatedAt)
            .ToListAsync();

        return View(notifications);
    }

    // STUDENT: Mark notification as read
    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        string email = User.Identity!.Name!;

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r =>
                r.Id == id &&
                r.BenildeEmail == email);

        if (reservation == null)
        {
            return NotFound();
        }

        reservation.IsNotificationRead = true;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Notifications));
    }

    // STUDENT: Count unread notifications
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UnreadNotificationCount()
    {
        string email = User.Identity!.Name!;

        int count = await _context.Reservations.CountAsync(r =>
            r.BenildeEmail == email &&
            r.StatusUpdatedAt != null &&
            !r.IsNotificationRead);

        return Json(new { count });
    }

    // ADMIN: Approve reservation
    [Authorize(Roles = "Admin")]
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
            reservation.IsNotificationRead = false;
            reservation.StatusUpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                "Reservation successfully approved. Student notification created.";
        }

        return RedirectToAction(nameof(Admin));
    }

    // ADMIN: Reject reservation
    [Authorize(Roles = "Admin")]
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
            reservation.IsNotificationRead = false;
            reservation.StatusUpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                "Reservation successfully rejected. Student notification created.";
        }

        return RedirectToAction(nameof(Admin));
    }

    // STUDENT OR ADMIN: View reservation confirmation
    public async Task<IActionResult> Confirmation(int id)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            return NotFound();
        }

        bool isAdmin = User.IsInRole("Admin");

        bool isOwner = string.Equals(
            reservation.BenildeEmail,
            User.Identity!.Name,
            StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isOwner)
        {
            return Forbid();
        }

        return View(reservation);
    }
}
