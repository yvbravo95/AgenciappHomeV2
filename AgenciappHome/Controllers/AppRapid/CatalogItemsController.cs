using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgenciappHome.Models;
using RapidMultiservice.Models.Responses;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using AgenciappHome.Controllers.Class;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace AgenciappHome.Controllers.AppRapid
{
    [Authorize]
    public class CatalogItemsController : Base
    {
        public CatalogItemsController(databaseContext context, IWebHostEnvironment env, Microsoft.Extensions.Options.IOptions<Settings> settings) : base(context, env, settings)
        {
        }
        // GET: CatalogItems
        public async Task<IActionResult> Index()
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            var databaseContext = _context.CatalogItems.Include(c => c.Agency).Include(c => c.LandingItem).Where(x => x.AgencyId == user.AgencyId);
            return ViewAutorize(new string[] { },await databaseContext.ToListAsync());
        }

        // GET: CatalogItems/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);

            var catalogItem = await _context.CatalogItems
                .Include(c => c.Agency)
                .Include(c => c.LandingItem)
                .FirstOrDefaultAsync(m => m.Id == id && m.AgencyId == user.AgencyId);
            if (catalogItem == null)
            {
                return NotFound();
            }

            return ViewAutorize(new string[] { },catalogItem);
        }

        // GET: CatalogItems/Create
        public IActionResult Create()
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);

            ViewData["AgencyId"] = user.AgencyId;
            ViewData["LandingItemId"] = new SelectList(_context.LandingItems.Where(x => x.AgencyId == user.AgencyId), "Id", "Name");
            ViewData["Provinces"] = new SelectList(_context.Provincia, "nombreProvincia", "nombreProvincia");
            return ViewAutorize(new string[] { },null);
        }

        // POST: CatalogItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CatalogItem catalogItem)
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);

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
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "CatalogItems";
                    filePath = Path.Combine(filePath, filename);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    catalogItem.ImageUrl = filename;
                }

                catalogItem.Id = Guid.NewGuid();
                var province = _context.Provincia.FirstOrDefault(x => x.nombreProvincia == catalogItem.Name);
                catalogItem.RefrenceId = province?.Id;
                _context.Add(catalogItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgencyId"] = user.AgencyId;
            ViewData["LandingItemId"] = new SelectList(_context.LandingItems.Where(x => x.AgencyId == user.AgencyId), "Id", "Name", catalogItem.LandingItemId);
            return ViewAutorize(new string[] { },catalogItem);
        }

        // GET: CatalogItems/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);

            var catalogItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.Id == id && x.AgencyId == user.AgencyId);
            if (catalogItem == null)
            {
                return NotFound();
            }
            ViewData["AgencyId"] = user.AgencyId;
            ViewData["LandingItemId"] = new SelectList(_context.LandingItems.Where(x => x.AgencyId == user.AgencyId), "Id", "Name", catalogItem.LandingItemId);
            ViewData["Provinces"] = new SelectList(_context.Provincia, "nombreProvincia", "nombreProvincia", catalogItem.Name);

            return ViewAutorize(new string[] { },catalogItem);
        }

        // POST: CatalogItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [FromForm] CatalogItem catalogItem)
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);

            if (id != catalogItem.Id)
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
                        string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "CatalogItems";

                        //Elimino la imagen anterior
                        if(catalogItem.ImageUrl != null)
                            System.IO.File.Delete(Path.Combine(filePath, catalogItem.ImageUrl));
                        //Creo una nueva
                        var auxName = file.FileName;
                        var arrName = auxName.Split('.');
                        string filename = date + '.' + arrName[1];
                        filePath = Path.Combine(filePath, filename);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        catalogItem.ImageUrl = filename;
                    }
                    var province = _context.Provincia.FirstOrDefault(x => x.nombreProvincia == catalogItem.Name);
                    catalogItem.RefrenceId = province?.Id;
                    _context.Update(catalogItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CatalogItemExists(catalogItem.Id))
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
            ViewData["AgencyId"] = user.AgencyId;
            ViewData["LandingItemId"] = new SelectList(_context.LandingItems.Where(x => x.AgencyId == user.AgencyId), "Id", "Name", catalogItem.LandingItemId);
            return ViewAutorize(new string[] { },catalogItem);
        }

        // GET: CatalogItems/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);

            var catalogItem = await _context.CatalogItems
                .Include(c => c.Agency)
                .Include(c => c.LandingItem)
                .FirstOrDefaultAsync(m => m.Id == id && m.AgencyId == user.AgencyId);
            if (catalogItem == null)
            {
                return NotFound();
            }

            return ViewAutorize(new string[] { },catalogItem);
        }

        // POST: CatalogItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var catalogItem = await _context.CatalogItems.FindAsync(id);
            _context.CatalogItems.Remove(catalogItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CatalogItemExists(Guid id)
        {
            return _context.CatalogItems.Any(e => e.Id == id);
        }

    }
}
