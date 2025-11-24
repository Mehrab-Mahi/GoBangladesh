using GoBangladesh.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Data;
using System.IO;
using GoBangladesh.Domain.Enums;

namespace GoBangladesh.Application.Services;

public class CommonExportService : ICommonExportService
{
    private readonly IFileService _fileService;
    private readonly string _connectionString;

    public CommonExportService(IFileService fileService,
        IConfiguration config)
    {
        _fileService = fileService;
        _connectionString = config.GetConnectionString(Enum.GetName(typeof(DbConnection), DbConnection.GoBangladeshConnection_Local)!);
    }

    public string ExportDataToExcel(string tripDashboardDataQuery, string folderName)
    {
        var excelFilePath = _fileService.GetExcelFilePath(folderName);
        ExcelPackage.License.SetNonCommercialPersonal(folderName);
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("export");
        using var dbConnection = new SqlConnection(_connectionString);

        if (dbConnection.State != ConnectionState.Open)
            dbConnection.Open();

        using var cmd = dbConnection.CreateCommand();
        cmd.CommandText = tripDashboardDataQuery;
        using var reader = cmd.ExecuteReader();

        var row = 1;

        for (var i = 0; i < reader.FieldCount; i++)
            sheet.Cells[row, i + 1].Value = reader.GetName(i);

        sheet.Cells[1, 1, 1, reader.FieldCount].Style.Font.Bold = true;
        row++;

        while (reader.Read())
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var val = reader.IsDBNull(i) ? string.Empty : reader.GetValue(i).ToString();
                sheet.Cells[row, i + 1].Value = val;
            }

            row++;
        }

        if (dbConnection.State != ConnectionState.Closed)
            dbConnection.Dispose();

        package.SaveAs(new FileInfo(excelFilePath.ServerPath));

        return excelFilePath.DownloadPath;
    }
}