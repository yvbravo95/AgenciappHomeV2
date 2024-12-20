using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class MinorAuthorization
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string MunicipalityOfBirth { get; set; }
        public string ProvinceOfBirth { get; set; }
        public string CountryOfBirth { get; set; }
        public string Ocuppation { get; set; }
        public string AddressStreet { get; set; }
        public string AddressMunicipality { get; set; }
        public string AddressProvince { get; set; }
        public string AddressCountry { get; set; }
        public string PassportNumber { get; set; }
        public string ChildFullName { get; set; }
        public string ChildMunicipalityOfBirth { get; set; }
        public string ChildProvinceOfBirth { get; set; }
        public string ChildCountryOfBirth { get; set; }
        public string ChildAddressStreet { get; set; }
        public string ChildAddressMunicipality { get; set; }
        public string ChildAddressProvince { get; set; }
        public string ChildAddressCountry { get; set; }
        public string ChildIdentityNumber { get; set; }
        public string ChildPassportNumber { get; set; }
        public string Notary { get; set; }
        public string DocumentCity { get; set; }
        public string DocumentCounty { get; set; }
        public string DocumentState { get; set; }
        public MaritalStatusEnum? MaritalStatus { get; set; }
        public MigratoryStatusEnum? MigratoryStatus { get; set; }

        public enum MaritalStatusEnum
        {
            [Description("Soltero(a)")] Single,
            [Description("Viudo(a)")] Widower,
            [Description("Casado(a)")] Married,
            [Description("Divorciado(a)")] Divorced
        }

        public enum MigratoryStatusEnum
        {
            [Description("Emigrado")] Emigrated,
            [Description("Residente en Cuba")] ResidentInCuba
        }
    }
}
