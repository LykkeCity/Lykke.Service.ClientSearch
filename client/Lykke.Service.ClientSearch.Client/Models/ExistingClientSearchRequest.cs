using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ClientSearch.Client.Model
{
    /// <summary>
    /// Search request class bu name and date of birth
    /// </summary>
    public class ExistingClientSearchRequest
    {
        /// <summary>
        /// Search request name property
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Search request date of birth property
        /// </summary>
        public DateTime DateOfBirth { get; set; }
    }
}
