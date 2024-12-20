using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgenciappHome.Models;
using RapidMultiservice.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using AgenciappHome.Controllers.Class;
using System.IO;

namespace AgenciappHome.Controllers
{
    [Authorize]
    public class LandingItemsController : Base
    {
        public LandingItemsController(databaseContext context, IWebHostEnvironment env, Microsoft.Extensions.Options.IOptions<Settings> settings) : base(context, env, settings)
        {
        }
       
        // GET: LandingItems
        public async Task<IActionResult> Index()
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            var databaseContext = _context.LandingItems.Include(l => l.Agency).Where(x => x.AgencyId == user.AgencyId);
            return ViewAutorize(new string[] { }, await databaseContext.ToListAsync());
        }

        // GET: LandingItems/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landingItem = await _context.LandingItems
                .Include(l => l.Agency)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (landingItem == null)
            {
                return NotFound();
            }

            return ViewAutorize(new string[] { }, landingItem);
        }

        // GET: LandingItems/Create
        public IActionResult Create()
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            ViewData["AgencyId"] = user.AgencyId;
            return ViewAutorize(new string[] { },null);
        }

        // POST: LandingItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] LandingItem landingItem)
        {
            if (ModelState.IsValid)
            {
                var file = Request.Form.Files.FirstOrDefault(x => x.Name == "image");
                var date = DateTime.Now.ToString("yMMddHHmmssff");
                string sWebRootFolder = _env.WebRootPath;

                if (file != null)
                {
                    var auxName = file.FileName;
                    var arrName = auxName.Split('.');
                    string filename = date + '.' + arrName[1];
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LandingItems";
                    filePath = Path.Combine(filePath, filename);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    landingItem.ImageUrl = filename;
                }


                landingItem.Id = Guid.NewGuid();
                _context.Add(landingItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgencyId"] = new SelectList(_context.Agency, "AgencyId", "LegalName", landingItem.AgencyId);
            return ViewAutorize(new string[] { },landingItem);
        }

        // GET: LandingItems/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landingItem = await _context.LandingItems.FindAsync(id);
            if (landingItem == null)
            {
                return NotFound();
            }
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            ViewData["AgencyId"] = user.AgencyId;
            return ViewAutorize(new string[] { },landingItem);
        }

        // POST: LandingItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,AgencyId,Name,ImageUrl")] LandingItem landingItem)
        {
            if (id != landingItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var file = Request.Form.Files.FirstOrDefault(x => x.Name == "image");
                    var date = DateTime.Now.ToString("yMMddHHmmssff");
                    string sWebRootFolder = _env.WebRootPath;

                    if (file != null)
                    {
                        string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LandingItems";

                        //Elimino la imagen anterior
                        if(landingItem.ImageUrl != null)
                            System.IO.File.Delete(Path.Combine(filePath, landingItem.ImageUrl));
                        //Creo una nueva
                        var auxName = file.FileName;
                        var arrName = auxName.Split('.');
                        string filename = date + '.' + arrName[1];
                        filePath = Path.Combine(filePath, filename);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        landingItem.ImageUrl = filename;
                    }

                    _context.Update(landingItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LandingItemExists(landingItem.Id))
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
            ViewData["AgencyId"] = new SelectList(_context.Agency, "AgencyId", "LegalName", landingItem.AgencyId);
            return ViewAutorize(new string[] { },landingItem);
        }

        // GET: LandingItems/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landingItem = await _context.LandingItems
                .Include(l => l.Agency)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (landingItem == null)
            {
                return NotFound();
            }

            return ViewAutorize(new string[] { } ,landingItem);
        }

        // POST: LandingItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            
            var landingItem = await _context.LandingItems.FindAsync(id);
            string sWebRootFolder = _env.WebRootPath;
            string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LandingItems";
            //Elimino la imagen anterior
            System.IO.File.Delete(Path.Combine(filePath, landingItem.ImageUrl));
            _context.LandingItems.Remove(landingItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LandingItemExists(Guid id)
        {
            return _context.LandingItems.Any(e => e.Id == id);
        }
    }
}
