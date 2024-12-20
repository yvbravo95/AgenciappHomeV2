using Agenciapp.Domain.Enums;
using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class InvoiceNote
    {
        protected InvoiceNote()
        {

        }

        public InvoiceNote(string serviceNumber, string description,decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
            NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, Order order)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            Order = order;
            Employee = employee;
            OrderDate = orderDate;
        }

        public InvoiceNote(string serviceNumber, string description, decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
            NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, EnvioMaritimo order)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            EnvioMaritimo = order;
            Employee = employee;
            OrderDate = orderDate;
        }

        public InvoiceNote(string serviceNumber, string description, decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
           NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, EnvioCaribe envioCaribe)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            EnvioCaribe = envioCaribe;
            Employee = employee;
            OrderDate = orderDate;
        }

        public InvoiceNote(string serviceNumber, string description, decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
           NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, OrderCubiq orderCubiq)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            OrderCubiq = orderCubiq;
            Employee = employee;
            OrderDate = orderDate;
        }

        public InvoiceNote(string serviceNumber, string description, decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
           NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, Passport passport)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            Passport = passport;
            Employee = employee;
            OrderDate = orderDate;
        }

        public InvoiceNote(string serviceNumber, string description, decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
           NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, Rechargue rechargue)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            Rechargue = rechargue;
            Employee = employee;
            OrderDate = orderDate;
        }

        public InvoiceNote(string serviceNumber, string description, decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
           NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, Remittance remittance)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            Remittance = remittance;
            Employee = employee;
            OrderDate = orderDate;
        }
        public InvoiceNote(string serviceNumber, string description, decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
           NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, Servicio servicio)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            Servicio = servicio;
            Employee = employee;
            OrderDate = orderDate;
        }
        
        public InvoiceNote(string serviceNumber, string description, decimal previousAmount, decimal amount, decimal previousCost, decimal cost,
           NoteType type, STipo serviceType, Agency agency, Client client, Wholesaler wholesaler, User employee, DateTime orderDate, Ticket ticket)
        {
            setNumber(type);
            CreatedAt = DateTime.UtcNow;
            ServiceNumber = serviceNumber;
            Description = description;
            PreviousAmount = previousAmount;
            Amount = amount;
            PreviousCost = previousCost;
            Cost = cost;
            Type = type;
            ServiceType = serviceType;
            Agency = agency;
            Client = client;
            Wholesaler = wholesaler;
            Ticket = ticket;
            Employee = employee;
            OrderDate = orderDate;
        }

        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime OrderDate { get; set; }
        public string NoteNumber { get; private set; }
        public string ServiceNumber { get; private set; }
        public string Description { get; private set; }
        public decimal Amount { get; private set; }
        public decimal PreviousAmount { get; private set; }
        public decimal Cost { get; private set; }
        public decimal PreviousCost { get; private set; }
        public NoteType Type { get; private set; }
        public STipo ServiceType { get; private set; }
        public User Employee { get; private set; }
        public Client Client { get; private set; }
        public Agency Agency { get; private set; }
        public Wholesaler Wholesaler { get; private set; }
        public Order Order { get; private set; }
        public EnvioMaritimo EnvioMaritimo { get; private set; }
        public EnvioCaribe EnvioCaribe { get; private set; }
        public OrderCubiq OrderCubiq { get; private set; }
        public Passport Passport { get; private set; }
        public Rechargue Rechargue { get; private set; }
        public Remittance Remittance { get; private set; }
        public Servicio Servicio { get; private set; }
        public Ticket Ticket { get; private set; }

        private void setNumber(NoteType type)
        {
            if (type == NoteType.NC)
                NoteNumber = "NC";
            else if (type == NoteType.ND)
                NoteNumber = "ND";

            NoteNumber += DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
