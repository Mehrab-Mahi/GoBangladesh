using System;

namespace GoBangladesh.Application.DTOs.Dashboard.Trip;

public class TripDashBoardTableData
{
    public string TripId { get; set; }
    public string OrganizationName { get; set; }
    public string Route { get; set; }
    public string BusNumber { get; set; }
    public string CardNumber { get; set; }
    public string PassengerName { get; set; }
    public DateTime TripStartTime { get; set; }
    public DateTime? TripEndTime { get; set; }
    public string StartingLatitude { get; set; }
    public string StartingLongitude { get; set; }
    public string EndingLatitude { get; set; }
    public string EndingLongitude { get; set; }
    public decimal Distance { get; set; }
    public decimal Fare { get; set; }
    public string Status { get; set; }
}