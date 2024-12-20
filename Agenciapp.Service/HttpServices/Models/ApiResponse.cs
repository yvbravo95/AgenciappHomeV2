﻿using System;

namespace Agenciapp.Service.HttpServices.Models
{
    public class ApiResponse
    {
        public bool Success => Error == null;

        public ApiError Error { get; set; }

        public void SetError(string status)
        {
            SetError(status, status);
        }

        public void SetError(Exception exception)
        {
            SetError("Unhandled", exception.Message);
        }

        public void SetError(string status, string message)
        {
            Error = new ApiError { Status = status, Message = message };
        }
    }
}
