using Agenciapp.Domain.Models;
using AgenciappHome.Controllers.Class;
using AgenciappHome.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Agenciapp.Common.Constants;
using Agenciapp.ApiClient.Security;
using Agenciapp.Common.Exceptions;
using Agenciapp.Common.Models.Security;
using Agenciapp.Common.Contrains;
using System.Security.Cryptography;

namespace AgenciappHome.Controllers
{
    public class AccountController : Base
    {
        private readonly SecurityLoginApi _loginApi;
        public AccountController(databaseContext context, IWebHostEnvironment env, 
            IOptions<Settings> settings, SecurityLoginApi loginApi) : base(context, env, settings)
        {
            _loginApi = loginApi;
        }

        [Authorize]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditarAgencia()
        {
            var role = AgenciaAuthorize.getRole(User, _context);
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();
            ViewBag.agencia = aAgency;
            ViewBag.ImagenPromotions = _context.ImagenPromotionRapidApp.Where(x => x.AgencyId == aAgency.AgencyId).ToList();
            ViewBag.ImagenPromotionPassports = _context.ImagenPromotionPassports.Where(x => x.AgencyId == aAgency.AgencyId).ToList();
            var elem = GetEnumSelectList<TypeImagenPromotion>();
            ViewBag.typeImage = new SelectList(elem.Select(x => new { a = x.Text, b = x.Value }), "a", "b");
            return ViewAutorize(new string[] { "Agencia" }, null);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditarAgencia(IFormCollection formCollection)
        {
            try
            {
                var role = AgenciaAuthorize.getRole(User, _context);
                var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
                var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();

                if (formCollection.Files.Count > 0)
                {
                    var image = formCollection.Files[0];
                    var date = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string sWebRootFolder = _env.WebRootPath;

                    var auxName = image.FileName;
                    var arrName = auxName.Split('.');
                    string filename = date + '.' + arrName[1];
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                    filePath = Path.Combine(filePath, filename);
                    // Verifico si la agencia ya tiene un logo
                    if (aAgency.logoName != null)
                    {
                        string pathdelete = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        pathdelete = Path.Combine(pathdelete, aAgency.logoName);
                        System.IO.File.Delete(pathdelete);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    aAgency.logoName = filename;
                    _context.Agency.Update(aAgency);
                    _context.SaveChanges();
                }
                var agencyInfo = formCollection.FirstOrDefault(x => x.Key == "agencyInfo");
                var fee = formCollection.FirstOrDefault(x => x.Key == "fee");
                var feeCombo = formCollection.FirstOrDefault(x => x.Key == "feeCombo");
                if (agencyInfo.Key != null)
                {
                    aAgency.AgencyInfo = agencyInfo.Value;
                    _context.Update(aAgency);

                }
                if (fee.Key != null)
                {
                    aAgency.creditCardFee = decimal.Parse(fee.Value);
                    _context.Update(aAgency);
                }
                if (feeCombo.Key != null)
                {
                    aAgency.creditCardFee_Combos = decimal.Parse(feeCombo.Value);
                    _context.Update(aAgency);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return RedirectToAction("EditarAgencia");

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddImagenPromotion(IFormCollection formCollection)
        {
            try
            {
                var role = AgenciaAuthorize.getRole(User, _context);
                var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
                var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();

                if (formCollection.Files.Count > 0)
                {
                    var image = formCollection.Files[0];
                    var date = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string sWebRootFolder = _env.WebRootPath;

                    var auxName = image.FileName;
                    var arrName = auxName.Split('.');
                    string filename = date + '.' + arrName[1];
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "Carousel";
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    filePath = Path.Combine(filePath, filename);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }
                    var idReference = formCollection.FirstOrDefault(x => x.Key == "idReference");
                    var typeImage = formCollection.FirstOrDefault(x => x.Key == "typeImage");

                    var imagePromotion = new ImagenPromotionRapidApp
                    {
                        AgencyId = aAgency.AgencyId,
                        ImagenPromotionId = Guid.NewGuid(),
                        Name = filename,
                        Url = "images/Carousel/" + filename,
                        TypeIdReference = idReference.Key != null ? (Guid?)Guid.Parse(idReference.Value) : null,
                        Type = (TypeImagenPromotion)Enum.Parse(typeof(TypeImagenPromotion), typeImage.Value.ToString(), true)
                    };
                    _context.Add(imagePromotion);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return RedirectToAction("EditarAgencia");

        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> RemoveImagenPromotion(Guid Id)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(x => x.Username == User.Identity.Name);
                var imagePromotion = await _context.ImagenPromotionRapidApp.FirstOrDefaultAsync(x => x.AgencyId == user.AgencyId && x.ImagenPromotionId == Id);
                if (imagePromotion == null)
                {
                    return Json(new { success = false, msg = "La imagen no existe" });
                }
                //Elimino la imagen
                string sWebRootFolder = _env.WebRootPath;
                string pathdelete = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "Carousel";
                pathdelete = Path.Combine(pathdelete, imagePromotion.Name);
                if(System.IO.File.Exists(pathdelete))
                    System.IO.File.Delete(pathdelete);

                _context.Remove(imagePromotion);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = "No se ha podido eliminar la imagen", exception = e.Message });

            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddImagenPromotionPassport(IFormCollection formCollection)
        {
            try
            {
                var role = AgenciaAuthorize.getRole(User, _context);
                var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
                var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();

                if (formCollection.Files.Count > 0)
                {
                    var image = formCollection.Files[0];
                    var date = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string sWebRootFolder = _env.WebRootPath;

                    var auxName = image.FileName;
                    var arrName = auxName.Split('.');
                    string filename = date + '.' + arrName[1];
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "CarouselPassport";
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    filePath = Path.Combine(filePath, filename);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    var imagePromotion = new ImagenPromotionPassport
                    {
                        AgencyId = aAgency.AgencyId,
                        ImagenPromotionId = Guid.NewGuid(),
                        Name = filename,
                        Url = "images/CarouselPassport/" + filename,
                    };
                    _context.Add(imagePromotion);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return RedirectToAction("EditarAgencia");

        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> RemoveImagenPromotionPassport(Guid Id)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(x => x.Username == User.Identity.Name);
                var imagePromotion = await _context.ImagenPromotionPassports.FirstOrDefaultAsync(x => x.AgencyId == user.AgencyId && x.ImagenPromotionId == Id);
                if (imagePromotion == null)
                {
                    return Json(new { success = false, msg = "La imagen no existe" });
                }
                //Elimino la imagen
                string sWebRootFolder = _env.WebRootPath;
                string pathdelete = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "CarouselPassport";
                pathdelete = Path.Combine(pathdelete, imagePromotion.Name);
                if (System.IO.File.Exists(pathdelete))
                    System.IO.File.Delete(pathdelete);

                _context.Remove(imagePromotion);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = "No se ha podido eliminar la imagen", exception = e.Message });

            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var t = _context.User.Where(x => x.Username == model.Username).Count();
            if (t > 0)
            {
                ModelState.AddModelError("Username", "Este usuario ya existe en el sistema. Introduzca uno distinto.");
            }
            else if (ModelState.IsValid)
            {
                var newuss = new User();
                newuss.UserId = Guid.NewGuid();
                newuss.Username = model.Username;
                newuss.Name = model.Firstname;
                newuss.LastName = model.Lastname;
                newuss.Email = model.Email;
                newuss.EmailConfirmed = true;
                var pm = gethash(model.Password);
                newuss.PasswordHash = pm.PasswordHash;
                newuss.PasswordSalt = Encoding.UTF8.GetString(pm.PasswordSalt, 0, pm.PasswordSalt.Length);
                newuss.Timestamp = DateTime.Now;
                newuss.Status = "Activo";
                newuss.Type = "Agencia";
                newuss.viewAdministracion = true;

                var magency = new Agency();
                magency.AgencyId = Guid.NewGuid();
                magency.CreatedAt = DateTime.Now;
                magency.Name = model.Name;
                magency.LegalName = model.LegalName;
                magency.Type = model.Type;
                magency.AgencyNumber = (int.Parse(_context.Agency.Where(x => !string.IsNullOrEmpty(x.AgencyNumber)).OrderByDescending(x => x.AgencyNumber).First().AgencyNumber) + 1).ToString("000");
                _context.Agency.Add(magency);

                var phone = new Phone();
                phone.PhoneId = Guid.NewGuid();
                phone.ReferenceId = magency.AgencyId;
                phone.Type = model.TypePhone;
                phone.Current = true;
                phone.Number = model.PhoneNumber;
                phone.ReferenceId = magency.AgencyId;
                _context.Phone.Add(phone);

                magency.Phone = phone;

                var address = new Address();
                address.AddressId = Guid.NewGuid();
                address.Country = model.Country;
                address.City = model.City;
                address.AddressLine1 = model.Address;
                address.Zip = model.Zip;
                address.Type = model.TypeAddress;
                address.State = model.State;
                address.Current = true;
                address.CreatedAt = DateTime.Now;
                address.CreatedBy = Guid.Empty;
                address.UpdatedAt = DateTime.Now;
                address.UpdatedBy = Guid.Empty;
                address.ReferenceId = magency.AgencyId;
                _context.Address.Add(address);

                //se crea una oficina por defecto
                var office = new Office();
                office.OfficeId = magency.AgencyId;
                office.Agency = magency;
                office.AgencyId = magency.AgencyId;
                office.OfficeAddress = address;
                office.OfficePhone = phone;
                office.Name = "Oficina " + magency.Name;
                _context.Office.Add(office);


                var userOffice = new UserOffice();
                userOffice.UserOfficeId = Guid.NewGuid();
                userOffice.UserId = newuss.UserId;
                userOffice.OfficeId = office.OfficeId;
                _context.UserOffice.Add(userOffice);

                newuss.AgencyId = magency.AgencyId;

                _context.User.Add(newuss);

                // Crear precios por tramite
                bool issuccess = preciosAgencia(magency.AgencyId);

                _context.SaveChanges();

                DataCookie.setCookie("AgenciappOfficeId", office.OfficeId.ToString(), Response);
                var loginResult = _loginApi.Authenticate(new Agenciapp.Common.Models.Security.AuthenticateModel(model.Username, model.Password));

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, newuss.Username));
                identity.AddClaim(new Claim(ClaimTypes.Name, newuss.Username));
                identity.AddClaim(new Claim(AppClaim.Token, loginResult.Token));
                identity.AddClaim(new Claim(AppClaim.TokenExpirationUtc, loginResult.ExpirationUtc.ToString()));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });

                Configuration.setNotificationByAgency(_context);

                return RedirectToAction("Index", "Home");


                //ModelState.Clear();
                //ViewBag.Message = user.Name + " " + user.LastName + " success registered";
            }
            return View();
        }

        private bool preciosAgencia(Guid Idagencia)
        {
            try
            {
                CostosxModulo precios = new CostosxModulo();
                precios.CostosxModuloId = Guid.NewGuid();
                precios.AgencyId = Idagencia;

                //Creo los valores por trámite
                string[] tramites = new[] { CategoryType.Auto, CategoryType.Pasaje, CategoryType.HotelVacaciones, CategoryType.PaqueteAereo, CategoryType.Maritimo, CategoryType.Remesa, CategoryType.Recarga, CategoryType.Cubiq, CategoryType.MaritimoAereo, CategoryType.CubiqAV, CategoryType.Rentadora };
                for (int i = 0; i < tramites.Length; i++)
                {
                    string tramite = tramites[i];
                    ValoresxTramite v = new ValoresxTramite();
                    v.ValoresxTramiteId = Guid.NewGuid();
                    v.CostosxModulo = precios;
                    v.Tramite = tramite;
                    // creo los valores por provincia
                    // obtengo las provincias
                    var provincias = _context.Provincia;
                    foreach (Provincia item in provincias)
                    {
                        ValorProvincia valorp = new ValorProvincia();
                        valorp.ValorProvinciaId = Guid.NewGuid();
                        valorp.provincia = item.nombreProvincia;
                        valorp.valor = 0;
                        if (v.Tramite == CostosxModulo.ModuloRemesas)
                        {
                            valorp.valor2 = 0;
                        }
                        valorp.listValores = v;
                        _context.ValorProvincia.Add(valorp);
                        v.valores.Add(valorp);
                    }
                    _context.ValoresxTramite.Add(v);
                    precios.valoresTramites.Add(v);
                }
                _context.CostosxModulo.Add(precios);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }


            return true;
        }

        public class PasswordModel
        {
            public string PasswordHash;
            public byte[] PasswordSalt;

            public PasswordModel(string PasswordHash, byte[] PasswordSalt)
            {
                this.PasswordHash = PasswordHash;
                this.PasswordSalt = PasswordSalt;
            }
        }

        public static PasswordModel gethash(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("HghMjh52Jn4Mn5m.51?lkm{m878Nj5fmjn0M5jjnK345jBHbgFv2N21KM");

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return new PasswordModel(hashed, salt);
        }

        public static bool isMathPassword(string password, string salt, string hash)
        {
            var bytesalt = Encoding.UTF8.GetBytes(salt);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: bytesalt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return (hash == hashed);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "")
        {
            //Preparar lista de acceso
            await AgenciaAuthorize.InitAccessList(_context);

            return View();
        }

        public class LoginData
        {
            [Required]
            public string Username { get; set; }

            [Required, DataType(DataType.Password)]
            public string PasswordHash { get; set; }

            public bool RememberMe { get; set; }
        }

        [BindProperty]
        public LoginData loginData { get; set; }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.User.Where(x => x.Username == model.Username);
                    if (user.Any())
                    {
                        var ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                        var connectionLog = new ConnectionLog
                        {
                            CreatedAt = DateTime.Now,
                            IpdAddress = ip,
                            User = user.FirstOrDefault()
                        };
                        _context.ConnectionLogs.Attach(connectionLog);
                        await _context.SaveChangesAsync();

                        var salt = user.FirstOrDefault().PasswordSalt;
                        var hash = user.FirstOrDefault().PasswordHash;
                        if (isMathPassword(model.Password, salt, hash))
                        {
                            

                            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, loginData.Username));
                            identity.AddClaim(new Claim(ClaimTypes.Name, loginData.Username));

                            try
                            {
                                var loginResult = _loginApi.Authenticate(new AuthenticateModel(model.Username, model.Password));
                                identity.AddClaim(new Claim(AppClaim.Token, loginResult.Token));
                                identity.AddClaim(new Claim(AppClaim.TokenExpirationUtc, loginResult.ExpirationUtc.ToString()));
                            }
                            catch { }
                           
                            var principal = new ClaimsPrincipal(identity);
                            if(user.FirstOrDefault().AgencyId == AgencyName.CubaBienTravel)
                            {
                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = loginData.RememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) });
                            }
                            else
                            {
                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = loginData.RememberMe });
                            }

                            //En caso de tener varias agencias mandarlo a seleccionar una
                            var agency = _context.Agency.Where(x => x.AgencyId == user.FirstOrDefault().AgencyId).FirstOrDefault();
                            var office = _context.Office.Where(x => x.AgencyId == agency.AgencyId);

                            DataCookie.setCookie("AgenciaName", agency.LegalName.ToString(), Response);
                            DataCookie.setCookie("IsCarga", _settings.IsMayoristaCarga(agency.AgencyId).ToString(), Response);
                            DataCookie.setCookie("AgencyId", agency.AgencyId.ToString(), Response);

                            //Si el rol es agencia
                            if (user.FirstOrDefault().Type == "Agencia")
                            {
                                if (office.Count() > 1) //y si tiene mas de una oficina
                                {
                                    return RedirectToAction("SelectAgency", "Account"); //mandar a seleccionar
                                }
                            }
                            //si el rol es empleado
                            else if (user.FirstOrDefault().Type == "Empleado" || user.FirstOrDefault().Type == "EmpleadoCuba" || user.FirstOrDefault().Type == "DistributorCuba" || user.FirstOrDefault().Type == "PrincipalDistributor")
                            {
                                //seleccionar la oficina por defecto de este
                                var userOffice = _context.UserOffice.Where(x => x.UserId == user.FirstOrDefault().UserId);
                                office = _context.Office.Where(x => x.OfficeId == userOffice.FirstOrDefault().OfficeId);
                            }


                            //Crea una cookie para al iniciar secion saber la agencia que selecciono
                            DataCookie.setCookie("AgenciappOfficeId", office.FirstOrDefault().OfficeId.ToString(), Response);
                            DataCookie.setCookie("OfficeName", office.FirstOrDefault().Name.ToString(), Response);

                            if (user.FirstOrDefault().Type == "EmpleadoCuba")
                            {
                                return RedirectToAction("indexempleado", "Home");
                            }
                            else if (user.FirstOrDefault().Type == "DistributorCuba")
                            {
                                return RedirectToAction("indexempleado", "Home");
                            }
                            else if (user.FirstOrDefault().Type == "PrincipalDistributor")
                            {
                                return RedirectToAction("indexempleado", "Home");
                            }
                            else if (user.FirstOrDefault().Type == "Empleado")
                            {
                                return RedirectToAction("indexempleado", "Home");
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Contraseña no válida");
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Usuario y contraseña no válidas");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Usuario y contraseña no válidas");
                    return View();
                }
            }
            catch(ApiClientException)
            {
                ModelState.AddModelError("", "Ha ocurrido un error. Usuario no autenticado");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> SelectAgency(string toReturn)
        {
            var user = _context.User.Where(x => x.Username == User.Identity.Name);
            var agency = _context.Agency.Where(x => x.AgencyId == user.FirstOrDefault().AgencyId).FirstOrDefault();
            var office = _context.Office.Where(x => x.AgencyId == agency.AgencyId);

            if (toReturn != null)
            {
                ViewData["toReturn"] = toReturn;
            }
            return View(office.ToList());
        }

        [HttpGet]
        public ActionResult LoadAgencyToSistem(Guid? id, string toReturn)
        {
            var office = _context.Office.Where(x => x.OfficeId == id);
            //Crea una cookie para al iniciar secion saber la agencia que selecciono
            DataCookie.setCookie("AgenciappOfficeId", office.FirstOrDefault().OfficeId.ToString(), Response);
            DataCookie.setCookie("OfficeName", office.FirstOrDefault().Name.ToString(), Response);

            if (toReturn != null)
            {
                return Redirect(toReturn);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public async Task<IActionResult> AccountSetting()
        {
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

            var userDistributors = _context.UserDistributors.Where(x => x.Distributor == user);
            var employee = _context.User.Where(x => x.AgencyId == user.AgencyId && (x.Type == "EmpleadoCuba" || x.Type == "DistributorCuba") && x.UserId != user.UserId);
            employee = employee.Union(userDistributors.Select(x => x.Employee));
            ViewBag.distributors = new MultiSelectList(employee, "UserId", "FullName", userDistributors.Select(x => x.Employee.UserId).ToArray());
            return ViewAutorize(new string[] { }, user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string currentpass, string password, string con_password)
        {
            if (password != con_password)
            {
                return new JsonResult(new { success = false, error = "Las contraseñas no coinciden" });
            }
            var aUser = await _context.User.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            var cp = gethash(currentpass);
            var cs = Encoding.UTF8.GetString(cp.PasswordSalt, 0, cp.PasswordSalt.Length);
            if (aUser.PasswordHash != cp.PasswordHash || aUser.PasswordSalt != cs)
                return new JsonResult(new { success = false, error = "Contraseñas incorrecta" });

            if (password != "")
            {
                var pm = gethash(password);
                aUser.PasswordHash = pm.PasswordHash;
                aUser.PasswordSalt = Encoding.UTF8.GetString(pm.PasswordSalt, 0, pm.PasswordSalt.Length);
                _context.User.Update(aUser);
                await _context.SaveChangesAsync();
            }
            return new JsonResult(new { success = true });

        }

        [HttpPost]
        [Authorize]
        public bool isMinoristaCubiq()
        {
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();
            var isMinor = _context.MinoristaCargas.Any(x => x.MinoristaId == aAgency.AgencyId);
            return isMinor || _settings.IsMayoristaCarga(aAgency.AgencyId);
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> getReferencesId(TypeImagenPromotion typeImage)
        {
            try
            {
                var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
                if (typeImage == TypeImagenPromotion.Landing)
                {
                    var landing = _context.LandingItems.Where(x => x.AgencyId == user.AgencyId).Select(x => new { id = x.Id, name = x.Name });
                    return Json(new { success = true, data = landing });
                }
                else if (typeImage == TypeImagenPromotion.Catalog)
                {
                    var catalog = _context.CatalogItems.Include(x => x.LandingItem).Where(x => x.AgencyId == user.AgencyId).Select(x => new { id = x.Id, name = $"{x.LandingItem.Name} - {x.Name}" });
                    return Json(new { success = true, data = catalog });
                }
                else if (typeImage == TypeImagenPromotion.Product)
                {
                    var products = _context.ProductosBodegas.Include(x => x.productoBodegaCatalogItems).Where(x => x.IdAgency == user.AgencyId && x.esVisible && x.productoBodegaCatalogItems.Any()).Select(x => new { id = x.IdProducto, name = x.Nombre });
                    return Json(new { success = true, data = products });
                }
                else
                {
                    return Json(new { success = true, data = new { } });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, exception = e.Message });
            }
        }

        public static IEnumerable<SelectListItem> GetEnumSelectList<T>()
        {
            return (Enum.GetValues(typeof(T)).Cast<T>().Select(
                enu => new SelectListItem() { Text = enu.ToString(), Value = enu.ToString() })).ToList();
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetUrlTourAdvisor()
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(x => x.Username == User.Identity.Name);
                var agency = await _context.Agency.FirstOrDefaultAsync(x => x.AgencyId == user.AgencyId);
                string urlTourAdvisor = _settings.UrlBookingTourAdvisor;
                string secret = _settings.SecretKeyTourAdvisor;

                //agregar query a la url
                urlTourAdvisor += $"?agencyname={agency.Name}&agencynumber={agency.Code}&username={user.Name}&userlastname={user.LastName}&userphone={user.Username}&userrole={user.Type}";

                // crear hash
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                string cadena = urlTourAdvisor + timestamp + secret;
                byte[] bytes = Encoding.UTF8.GetBytes(cadena);
                byte[] hash;
                using (SHA256 sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(bytes);
                }
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                string url = urlTourAdvisor + "&timestamp=" + timestamp + "&hash=" + sb.ToString();
                return Json(new { success = true, url });
            }
            catch(Exception e)
            {
                return Json(new { success = false, exception = e.Message });
            }
        }
    }
}