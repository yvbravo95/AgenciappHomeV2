namespace Agenciapp.Common.Headers
{
    public class Header
    {
        public Guid? AgencyId { get; set; }
        public Guid? UserId { get; set; }

        /// <summary>
        /// Authorization Token
        /// </summary>
        public string Token { get; set; }
    }
}
