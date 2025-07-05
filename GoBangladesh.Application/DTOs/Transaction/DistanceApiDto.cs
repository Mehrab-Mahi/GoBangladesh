using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Transaction;

public class DistanceApiDto
{
    public List<Route> Routes { get; set; }
}

public class Route
{
    public decimal Distance { get; set; }
}