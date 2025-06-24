using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces
{
    public interface IMediaService
    {
        PayloadResponse UploadCampaignMedia(MediaVm mediaData);
        MediaDataVm GetAllMedia(MediaDataSizeVm mediaDataSize);
        PayloadResponse DeleteCampaignMedia(MediaDeleteVm mediaDeleteData);
        MediaDataWithIdVm GetCampaignMedia(string campaignId);
    }
}