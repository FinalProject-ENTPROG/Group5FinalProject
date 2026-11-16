using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using firstASP.Data;
using firstASP.Models;

namespace firstASP.Controllers;

[Authorize(Roles = "Student")]
public class StudentProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Display student profile
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new StudentProfile();
        }

        return View(profile);
    }

    // Save or update student profile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(StudentProfile profile)
    {
        string userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        ModelState.Remove(nameof(StudentProfile.UserId));

        if (!ModelState.IsValid)
        {
            return View(profile);
        }

        var existingProfile = await _context.StudentProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (existingProfile == null)
        {
            profile.UserId = userId;
            profile.UpdatedAt = DateTime.Now;

            _context.StudentProfiles.Add(profile);
        }
        else
        {
            existingProfile.StudentIdNumber = profile.StudentIdNumber;
            existingProfile.FullName = profile.FullName;
            existingProfile.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        TempData["ProfileMessage"] = "Your profile has been saved successfully.";

        return RedirectToAction(nameof(Index));
    }
}