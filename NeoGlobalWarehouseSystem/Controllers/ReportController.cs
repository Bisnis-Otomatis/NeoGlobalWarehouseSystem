using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoGlobalWarehouseSystem.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace NeoGlobalWarehouseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogger<ReportController> _logger;
        public ReportController(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<ReportController> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string type, [FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var transactions = await db.Transactions
                    .Include(x => x.TransactionProducts)
                    .Include(x => x.ProcessedBy)
                    .Include(x => x.Teacher)
                    .Where(x => x.TimeStamp.Month == month && x.TimeStamp.Year == year)
                    .ToListAsync();

                if (type.ToLower() == "customer")
                {
                    transactions = transactions.Where(x => x.TeacherId == null).ToList();
                }
                else if (type.ToLower() == "teacher")
                {
                    transactions = transactions.Where(x => x.TeacherId != null).ToList();
                }
                else
                {
                    _logger.LogWarning("Invalid report type: {Type}", type);
                    return BadRequest("Invalid report type");
                }

                if (!transactions.Any())
                {
                    _logger.LogWarning("No transactions found for type={Type}, month={Month}, year={Year}", type, month, year);
                    return NotFound("No transactions found for the selected period and type.");
                }

                var pdfBytes = GenerateReportPdf(type, transactions, month, year);
                var fileName = $"Report_{type}_{year}_{month}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report PDF");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private byte[] GenerateReportPdf(string type, List<Data.ApplicationDb.Transaction> transactions, int month, int year)
        {
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            var reportTitle = type.ToLower() == "customer" ? "Customer Sales Report" : "Teacher Product Take Report";
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text($"{reportTitle} - {monthName} {year}").FontSize(18).Bold();
                    page.Content().Element(c =>
                    {
                        c.Column(col =>
                        {
                            foreach (var tx in transactions.OrderBy(x => x.TimeStamp))
                            {
                                col.Item().Text($"Date: {tx.TimeStamp:dd/MM/yyyy HH:mm}").Bold();
                                if (type.ToLower() == "customer")
                                {
                                    col.Item().Text($"Customer: {tx.CustomerName}");
                                }
                                else
                                {
                                    col.Item().Text($"Teacher: {tx.Teacher?.Name}");
                                }
                                col.Item().Text($"Processed By: {tx.ProcessedBy.Name}");
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(); // Product
                                        columns.ConstantColumn(60); // Qty
                                        if (type.ToLower() == "customer")
                                            columns.ConstantColumn(80); // Price
                                        columns.ConstantColumn(90); // Total
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Product").Bold();
                                        header.Cell().Text("Qty").Bold();
                                        if (type.ToLower() == "customer")
                                            header.Cell().Text("Price").Bold();
                                        header.Cell().Text(type.ToLower() == "customer" ? "Total" : "");
                                    });
                                    foreach (var p in tx.TransactionProducts)
                                    {
                                        table.Cell().Text(p.Name);
                                        table.Cell().Text(p.Quantity.ToString());
                                        if (type.ToLower() == "customer")
                                            table.Cell().Text($"Rp. {p.Price:N0}");
                                        table.Cell().Text(type.ToLower() == "customer" ? $"Rp. {(p.Price * p.Quantity):N0}" : "");
                                    }
                                });
                                if (type.ToLower() == "customer")
                                {
                                    var total = tx.TransactionProducts.Sum(x => x.Price * x.Quantity);
                                    col.Item().AlignRight().Text($"Total: Rp. {total:N0}").Bold();
                                }
                                col.Item().PaddingBottom(10);
                            }
                        });
                    });
                });
            });
            return document.GeneratePdf();
        }
    }
}
