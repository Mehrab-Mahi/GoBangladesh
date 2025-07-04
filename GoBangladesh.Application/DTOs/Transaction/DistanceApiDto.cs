using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Transaction;

public class DistanceApiDto
{
    public List<Feature> Features { get; set; }
}

public class Feature
{
    public Properties Properties { get; set; }
}

public class Properties
{
    public Summary Summary { get; set; }
}

public class Summary
{
    public decimal Distance { get; set; }
    public decimal Duration { get; set; }
}