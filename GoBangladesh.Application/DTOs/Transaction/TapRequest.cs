using System;

namespace GoBangladesh.Application.DTOs.Transaction;

public class TapRequest
{
    public string CardNumber { get; set; }
    public string SessionId { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}