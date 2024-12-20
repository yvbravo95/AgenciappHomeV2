using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxMinoristasCostos
    {
        private List<Agency> minoristas;
        private List<Provincia> provincias;
        private List<CostoxModuloMayorista> costoxModulo;
        private databaseContext _context;
        public AuxMinoristasCostos(Guid idMayorista, databaseContext _context)
        {
            this._context = _context;
            costoxModulo = this._context.CostoxModuloMayorista.Include(x => x.valoresTramites).Where(x => x.AgencyMayoristaId == idMayorista).ToList();

            minoristas = new List<Agency>();

            foreach (var item in costoxModulo)
            {
                this.minoristas.Add(_context.Agency.Find(item.AgencyId));
            }

            this.provincias = this._context.Provincia.ToList();
        }

        public AuxMinoristasCostos(Guid idMayorista, Guid idMinorista, databaseContext _context)
        {
            this._context = _context;
            costoxModulo = this._context.CostoxModuloMayorista
                .Include(x => x.valoresTramites)
                .Where(x => x.AgencyMayoristaId == idMayorista && x.AgencyId == idMinorista).ToList();

            minoristas = new List<Agency>();

            foreach (var item in costoxModulo)
            {
                this.minoristas.Add(_context.Agency.Find(item.AgencyId));
            }

            this.provincias = this._context.Provincia.ToList();
        }

        public List<List<string>> getValores(string tramite)
        {
            List<List<string>> valores = new List<List<string>>();
            // Añado los nombres de las agencias
            valores.Add(this.minoristas.Select(x => x.Name).ToList());

            foreach (var item in this.provincias)
            {
                List<string> val = new List<string>();
                val.Add(item.nombreProvincia);
                valores.Add(val);
            }
            foreach (var costo in this.costoxModulo)
            {
                ValoresxTramite valorTramite = costo.valoresTramites.Where(x => x.Tramite == tramite).FirstOrDefault();
                if (valorTramite != null)
                {
                    var valorProvincia = _context.ValorProvincia.Where(x => x.listValores.ValoresxTramiteId == valorTramite.ValoresxTramiteId).ToList();
                    foreach (var valP in valorProvincia)
                    {
                        string v = "";
                        if (tramite == "Remesa" || tramite == "Paquete Aereo" || tramite == "Cubiq" || tramite == "CubiqAV" || tramite == "Maritimo")
                        {
                            if (tramite == "Remesa")
                            {
                                v = valP.valor.ToString("0.00") + " | " + valP.valor2.ToString("0.00") + " | " + valP.valor3.ToString("0.00") + " | " + valP.valor4.ToString("0.00") + " | " + valP.valor5.ToString("0.00") + " | " + valP.valor6.ToString("0.00");

                            }
                            else if(tramite == "CubiqAV")
                            {
                                v = valP.valor.ToString("0.00") + " | " + valP.valor2.ToString("0.00") + " | " + valP.valor3.ToString("0.00") + " | " + valP.valor4.ToString("0.00");
                            }
                            else
                            {
                                v = valP.valor.ToString("0.00") + " | " + valP.valor2.ToString("0.00");

                            }
                        }
                        else
                        {
                            v = valP.valor.ToString("0.00", CultureInfo.InvariantCulture);
                        }
                        List<string> elemento = valores.Where(x => x.Contains(valP.provincia)).FirstOrDefault();
                        elemento.Add(v);
                    }
                }

            }
            return valores;
        }
    }


}
