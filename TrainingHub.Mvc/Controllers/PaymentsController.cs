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
    public class PaymentsController : Controller
    {
        private readonly TrainingHubDbContext _context;

        public PaymentsController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var trainingHubDbContext = _context.Payments.Include(p => p.Enrollment).Include(p => p.Trainee);
            return View(await trainingHubDbContext.ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Enrollment)
                .Include(p => p.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "AttendanceStatus");
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email");
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TraineeId,EnrollmentId,AmountPaid,PaidAt,PaymentMethod,ReferenceNumber,Notes")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "AttendanceStatus", payment.EnrollmentId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", payment.TraineeId);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "AttendanceStatus", payment.EnrollmentId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", payment.TraineeId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TraineeId,EnrollmentId,AmountPaid,PaidAt,PaymentMethod,ReferenceNumber,Notes")] Payment payment)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
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
            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "AttendanceStatus", payment.EnrollmentId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", payment.TraineeId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Enrollment)
                .Include(p => p.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
                return NotFound();

            try
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete payment because related records exist.");
                return View("Delete", payment);
            }
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
