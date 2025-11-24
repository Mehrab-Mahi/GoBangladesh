using GoBangladesh.Application.DTOs.Export;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IExportService
{
    PayloadResponse ExportTripHistoryData(TripHistoryExportFilterDto model);
    PayloadResponse ExportSessionHistoryData(SessionHistoryExportFilterDto model);
    PayloadResponse ExportBusList(BusListExportFilterDto model);
    PayloadResponse ExportCardList(CardListExportFilterDto model);
    PayloadResponse ExportPassengerList(PassengerListExportFilterDto model);
    PayloadResponse ExportPromoList(PromoListExportFilterDto model);
    PayloadResponse ExportCardStatement(RechargeReturnHistoryExportFilterDto model);
    PayloadResponse DeleteExportedFile(string filePath);
}