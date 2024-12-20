using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AgenciappHome.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Agenciapp.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agenciapp.Domain.Models
{
    public class Pallet
    {
        public const string STATUS_INICIADO = "Iniciado";
        public const string STATUS_COMPLETADO = "Completado";

        public Pallet()
        {
            Number = $"PL{DateTime.Now.ToString("yyMMddhhmm")}";
            Status = STATUS_INICIADO;
            CreatedAt = DateTime.UtcNow;
            UpdateAt = DateTime.UtcNow;
            Type = string.Empty;
            Packages = new List<Paquete>();
            QtyPallets = 1;
        }

        public Pallet(Agency agency, int qtyPallets)
        {
            Number = $"PL{DateTime.Now.ToString("yyMMddhhmm")}";
            Status = STATUS_INICIADO;
            CreatedAt = DateTime.UtcNow;
            UpdateAt = DateTime.UtcNow;
            Type = "";
            Agency = agency;
            Packages = new List<Paquete>();
            QtyPallets = qtyPallets;
        }

        public Pallet(string number, string status, string type, Agency agency, List<Paquete> packages, int qtyPallets)
        {
            Number = number;
            Status = status;
            CreatedAt = DateTime.UtcNow;
            UpdateAt = DateTime.UtcNow;
            Type = type;
            Agency = agency;
            Packages = packages;
            QtyPallets = qtyPallets;
        }

        public void ChangeStatus(string status)
        {
            Status = status;
        }

        public void ChangeType(string type)
        {
            Type = type;
        }

        public void AddPackage(Paquete paquete)
        {
            if(string.IsNullOrEmpty(Type))
            {
               ChangeType(paquete.OrderCubiq.Type);
            }

            Packages.Add(paquete);
        }

        [Key] public Guid Id { get; set; }
        public Guid? GuiaId { get; set; }
        public string Number { get; private set; }
        public string Status { get; private set; }
        public string Type { get; private set; }
        public int QtyPallets { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdateAt { get; private set; }
        public Agency Agency { get; set; }
        [ForeignKey("GuiaId")]
        public GuiaAerea Guia { get; set; }
        public List<Paquete> Packages { get; set; }
    }
}