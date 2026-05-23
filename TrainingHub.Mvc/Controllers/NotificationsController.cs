using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;
using TrainingHub.Security;

namespace TrainingHub.Mvc.Controllers
{
    [Authorize(Roles = RoleNames.TrainingCoordinator + "," + RoleNames.Instructor)]
    public class NotificationsController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public NotificationsController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var trainingHubDbContext = _context.Notifications.Include(n => n.Instructor).Include(n => n.Trainee);
            return View(await trainingHubDbContext.ToListAsync());
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Instructor)
                .Include(n => n.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            // If the notification links to a target record that doesn't exist, provide graceful message
            if (!string.IsNullOrEmpty(notification.Link))
            {
                // naive handling: if link contains known controllers/ids, try to detect
                // For example link: "/CourseSessions/Details/5"
                try
                {
                    var parts = notification.Link.Trim('/').Split('/');
                    if (parts.Length >= 3)
                    {
                        var controller = parts[0];
                        var action = parts[1];
                        if (int.TryParse(parts[2], out var targetId))
                        {
                            // Basic checks for common types
                            if (string.Equals(controller, "CourseSessions", StringComparison.OrdinalIgnoreCase))
                            {
                                var cs = await _context.CourseSessions.FindAsync(targetId);
                                if (cs == null)
                                {
                                    return RedirectToAction("NotFoundTarget", "Notifications");
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore parsing errors and show notification normally
                }
            }

            return View(notification);
        }

        // GET: Notifications/Create
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public IActionResult Create()
        {
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio");
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email");
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Message,Type,CreatedAt,IsRead,TraineeId,InstructorId")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                // Prevent duplicate rapid notifications: do not create if an identical recent notification exists
                var tenSecondsAgo = DateTime.Now.AddSeconds(-10);
                var duplicate = await _context.Notifications.AnyAsync(n =>
                    n.Title == notification.Title &&
                    n.Message == notification.Message &&
                    n.Type == notification.Type &&
                    n.TraineeId == notification.TraineeId &&
                    n.InstructorId == notification.InstructorId &&
                    n.CreatedAt >= tenSecondsAgo);

                if (!duplicate)
                {
                    notification.CreatedAt = DateTime.Now;
                    _context.Add(notification);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio", notification.InstructorId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", notification.TraineeId);
            return View(notification);
        }

        // GET: Notifications/Edit/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio", notification.InstructorId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", notification.TraineeId);
            return View(notification);
        }

        // POST: Notifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Message,Type,CreatedAt,IsRead,TraineeId,InstructorId")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
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
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio", notification.InstructorId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", notification.TraineeId);
            return View(notification);
        }

        // GET: Notifications/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Instructor)
                .Include(n => n.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound();

            try
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete notification.");
                return View("Delete", notification);
            }
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }

        // Redirect target when notification points to a deleted/modified record
        public IActionResult NotFoundTarget()
        {
            return View();
        }
    }
}
