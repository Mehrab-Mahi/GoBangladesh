using System.Collections.Generic;

namespace GoBangladesh.Application.ViewModels;

public class OfficialLeaderDto
{
    public List<LeaderDataDto> DcOfficeLeaders { get; set; }
    public List<LeaderDataDto> CivilOfficeLeaders { get; set; }
}