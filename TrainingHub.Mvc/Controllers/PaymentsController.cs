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
    [Authorize(Roles = RoleNames.TrainingCoordinator + "," + RoleNames.Trainee)]
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
            var currentEmail = User.Identity?.Name;
            IQueryable<Payment> trainingHubDbContext = _context.Payments
                .Include(p => p.Enrollment)
                    .ThenInclude(e => e.CourseSession)
                        .ThenInclude(cs => cs.Course)
                .Include(p => p.Trainee);

            if (User.IsInRole(RoleNames.Trainee) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                trainingHubDbContext = trainingHubDbContext.Where(payment => payment.Trainee != null && payment.Trainee.Email == currentEmail);
            }

            var payments = await trainingHubDbContext.ToListAsync();
            await ApplyPaymentSummariesAsync(payments);
            return View(payments);
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
                    .ThenInclude(e => e.CourseSession)
                        .ThenInclude(cs => cs.Course)
                .Include(p => p.Trainee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            if (User.IsInRole(RoleNames.Trainee) && !User.IsInRole(RoleNames.TrainingCoordinator))
            {
                if (!string.Equals(payment.Trainee?.Email, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }

            await ApplyPaymentSummariesAsync(new[] { payment });

            return View(payment);
        }

        // GET: Payments/Create
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        public IActionResult Create()
        {
            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "AttendanceStatus");
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email");
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TraineeId,EnrollmentId,AmountPaid,PaidAt,PaymentMethod,ReferenceNumber,Notes")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                // Validate against remaining balance for the enrollment
                var remaining = await GetRemainingBalanceAsync(payment.EnrollmentId);
                if (remaining == null)
                {
                    ModelState.AddModelError("", "Enrollment or related course not found.");
                }
                else if (payment.AmountPaid > remaining.Value)
                {
                    ModelState.AddModelError(nameof(payment.AmountPaid), $"Payment exceeds remaining balance of {remaining.Value:C}.");
                }

                if (ModelState.ErrorCount == 0)
                {
                    _context.Add(payment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "AttendanceStatus", payment.EnrollmentId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", payment.TraineeId);
            return View(payment);
        }

        // GET: Payments/Edit/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
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
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
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
                // Validate against remaining balance for the enrollment excluding this payment
                var remaining = await GetRemainingBalanceAsync(payment.EnrollmentId, payment.Id);
                if (remaining == null)
                {
                    ModelState.AddModelError("", "Enrollment or related course not found.");
                }
                else if (payment.AmountPaid > remaining.Value)
                {
                    ModelState.AddModelError(nameof(payment.AmountPaid), $"Payment exceeds remaining balance of {remaining.Value:C}.");
                }

                if (ModelState.ErrorCount == 0)
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
            }
            ViewData["EnrollmentId"] = new SelectList(_context.Enrollments, "Id", "AttendanceStatus", payment.EnrollmentId);
            ViewData["TraineeId"] = new SelectList(_context.Trainees, "Id", "Email", payment.TraineeId);
            return View(payment);
        }

        // Returns remaining balance for the enrollment's course fee.
        // If excludePaymentId is provided, that payment is omitted from the total paid (useful when editing).
        private async Task<decimal?> GetRemainingBalanceAsync(int enrollmentId, int? excludePaymentId = null)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSession)
                    .ThenInclude(cs => cs.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null || enrollment.CourseSession == null || enrollment.CourseSession.Course == null)
                return null;

            var fee = enrollment.CourseSession.Course.Fee;

            var totalPaid = await _context.Payments
                .Where(p => p.EnrollmentId == enrollmentId && (!excludePaymentId.HasValue || p.Id != excludePaymentId.Value))
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;

            var remaining = fee - totalPaid;
            if (remaining < 0) remaining = 0m;
            return remaining;
        }

        private async Task ApplyPaymentSummariesAsync(IEnumerable<Payment> payments)
        {
            foreach (var payment in payments)
            {
                var remaining = await GetRemainingBalanceAsync(payment.EnrollmentId, payment.Id);
                payment.OutstandingBalance = remaining ?? 0m;
                payment.IsOverdue = payment.OutstandingBalance > 0m && payment.PaidAt.Date < DateTime.UtcNow.Date;
            }
        }

        // GET: Payments/Delete/5
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
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
        [Authorize(Roles = RoleNames.TrainingCoordinator)]
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
