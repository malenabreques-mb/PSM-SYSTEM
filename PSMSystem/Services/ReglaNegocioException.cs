using System;

namespace PSMSystem.Services;


public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string message) : base(message)
    {
    }
}
