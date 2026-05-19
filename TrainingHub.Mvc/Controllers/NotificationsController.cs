using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;

namespace TrainingHub.Mvc.Controllers
{
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

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio");
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email");
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Message,Type,CreatedAt,IsRead,TraineeId,InstructorId")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "Bio", notification.InstructorId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", notification.TraineeId);
            return View(notification);
        }

        // GET: Notifications/Edit/5
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
    }
}
