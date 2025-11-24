using Microsoft.Data.SqlClient;

namespace GoBangladesh.Application.Interfaces;

public interface ICommonExportService
{
    string ExportDataToExcel(string tripDashboardDataQuery, string folderName);
}