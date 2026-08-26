using System;

namespace PSMSystem.Services;


public record ResultadoPagina<T>(IReadOnlyList<T> Items, int Total);
