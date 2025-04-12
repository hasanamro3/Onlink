using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Onlink.Data;
using Onlink.Models;

namespace Onlink.Controllers
{
    public class CheckInfoesController : Controller
    {
        private readonly DataContext _context;

        public CheckInfoesController(DataContext context)
        {
            _context = context;
        }

        // GET: CheckInfoes
        public async Task<IActionResult> Index()
        {
            var dataContext = _context.CheckInfo.Include(c => c.Employee);
            return View(await dataContext.ToListAsync());
        }

        // GET: CheckInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkInfo = await _context.CheckInfo
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(m => m.CheckInfoId == id);
            if (checkInfo == null)
            {
                return NotFound();
            }

            return View(checkInfo);
        }

        // GET: CheckInfoes/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employee, "EmployeeId", "Email");
            return View();
        }

        // POST: CheckInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CheckInfoId,EmployeeId")] CheckInfo checkInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(checkInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employee, "EmployeeId", "Email", checkInfo.EmployeeId);
            return View(checkInfo);
        }

        // GET: CheckInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkInfo = await _context.CheckInfo.FindAsync(id);
            if (checkInfo == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employee, "EmployeeId", "Email", checkInfo.EmployeeId);
            return View(checkInfo);
        }

        // POST: CheckInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CheckInfoId,EmployeeId")] CheckInfo checkInfo)
        {
            if (id != checkInfo.CheckInfoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(checkInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CheckInfoExists(checkInfo.CheckInfoId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employee, "EmployeeId", "Email", checkInfo.EmployeeId);
            return View(checkInfo);
        }

        // GET: CheckInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkInfo = await _context.CheckInfo
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(m => m.CheckInfoId == id);
            if (checkInfo == null)
            {
                return NotFound();
            }

            return View(checkInfo);
        }

        // POST: CheckInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checkInfo = await _context.CheckInfo.FindAsync(id);
            if (checkInfo != null)
            {
                _context.CheckInfo.Remove(checkInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CheckInfoExists(int id)
        {
            return _context.CheckInfo.Any(e => e.CheckInfoId == id);
        }
    }
}
