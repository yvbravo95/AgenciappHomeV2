using System;

namespace Agenciapp.Common.Models.Dto
{
    public class ShippingItemDto
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Numero del Bolsa
        /// </summary>
        public string BagNumber { get; set; }
        /// <summary>
        /// Numero de Orden
        /// </summary>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Tipo de Tramite
        /// </summary>
        public string OrderType { get; set; }
        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Provincia del Contacto
        /// </summary>
        public string ContactProvince { get; set; }
        public bool isSend { get; set; }
        public bool isReview { get; set; }

        /// <summary>
        /// Cantidad del producto en el equipaje
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Cantidad real del producto
        /// </summary>
        public int TotalQuantity { get; set; }
    }
}