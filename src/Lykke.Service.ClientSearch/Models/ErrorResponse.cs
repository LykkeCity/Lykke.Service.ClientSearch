using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lykke.Service.ClientSearch.Models
{
    public class ErrorResponse
    {
        public string ErrorMessage { get; }

        private ErrorResponse() :
            this(null)
        {
        }

        private ErrorResponse(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }


        public static ErrorResponse Create()
        {
            return new ErrorResponse();
        }


        public static ErrorResponse Create(string message)
        {
            return new ErrorResponse(message);
        }
    }
}
