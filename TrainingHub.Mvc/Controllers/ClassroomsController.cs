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
    [Authorize(Roles = RoleNames.TrainingCoordinator)]
    public class ClassroomsController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public ClassroomsController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: Classrooms
        public async Task<IActionResult> Index()
        {
            return View(await _context.Classrooms.ToListAsync());
        }

        // GET: Classrooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classroom == null)
            {
                return NotFound();
            }

            return View(classroom);
        }

        // GET: Classrooms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classrooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomCode,Capacity,Location,HasProjector,HasLabComputer,IsAvailable")] Classroom classroom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(classroom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(classroom);
        }

        // GET: Classrooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }
            return View(classroom);
        }

        // POST: Classrooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomCode,Capacity,Location,HasProjector,HasLabComputer,IsAvailable")] Classroom classroom)
        {
            if (id != classroom.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(classroom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassroomExists(classroom.Id))
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
            return View(classroom);
        }

        // GET: Classrooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classroom == null)
            {
                return NotFound();
            }

            return View(classroom);
        }

        // POST: Classrooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.CourseSessions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null)
                return NotFound();

            if (classroom.CourseSessions.Any())
            {
                ModelState.AddModelError("", "Cannot delete this classroom because it is assigned to one or more course sessions.");
                return View("Delete", classroom);
            }

            try
            {
                _context.Classrooms.Remove(classroom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete this classroom because it is connected to other records.");
                return View("Delete", classroom);
            }
        }

        private bool ClassroomExists(int id)
        {
            return _context.Classrooms.Any(e => e.Id == id);
        }
    }
}
