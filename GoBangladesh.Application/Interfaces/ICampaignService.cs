using System.Collections.Generic;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces
{
    public interface ICampaignService
    {
        PayloadResponse Create(CampaignVm campaignData);
        PayloadResponse Update(CampaignVm campaignData);
        PayloadResponse Delete(string id);
        CampaignVm Get(string id);
        object GetAll(int pageNo, int pageSize);
        object GetRunningAndUpcomingCampaign(int pageNo, int pageSize);
        object GetVolunteerPermittedCampaigns(int pageNo, int pageSize);
    }
}