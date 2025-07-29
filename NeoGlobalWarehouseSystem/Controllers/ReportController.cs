using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoGlobalWarehouseSystem.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.IO;

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
                    .Include(x => x.Employee)
                    .Where(x => x.TimeStamp.Month == month && x.TimeStamp.Year == year)
                    .ToListAsync();

                if (type.ToLower() == "customer")
                {
                    transactions = transactions.Where(x => x.EmployeeId == null).ToList();
                }
                else if (type.ToLower() == "employee")
                {
                    transactions = transactions.Where(x => x.EmployeeId != null).ToList();
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
            var reportTitle = type.ToLower() == "customer" ? "Customer Sales Report" : "Employee Product Take Report";
            // Hitung grand total untuk customer
            long grandTotal = 0;
            if (type.ToLower() == "customer")
            {
                grandTotal = transactions.Sum(tx => tx.TransactionProducts.Sum(x => x.Price * x.Quantity));
            }
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().AlignCenter().Text($"{reportTitle} - {monthName} {year}").FontSize(18).Bold();
                        headerCol.Item().AlignCenter().Text($"Date: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(12);
                    });
                    page.Content().Element(c =>
                    {
                        c.Column(col =>
                        {
                            bool first = true;
                            foreach (var tx in transactions.OrderBy(x => x.TimeStamp))
                            {
                                if (!first)
                                {
                                    // Add a thick horizontal line to separate transactions
                                    col.Item().PaddingVertical(10).BorderBottom(2).BorderColor(Colors.Grey.Darken2);
                                }
                                first = false;
                                col.Item().Text($"Date: {tx.TimeStamp:dd/MM/yyyy HH:mm}").Bold();
                                if (type.ToLower() == "customer")
                                {
                                    col.Item().Text($"Customer: {tx.CustomerName}");
                                }
                                else
                                {
                                    // Format: Nama (EmployeeType) dengan spasi sebelum kurung
                                    var employeeName = tx.Employee?.Name ?? "";
                                    var employeeType = tx.Employee?.Type.ToString() ?? "";
                                    col.Item().Text($"Employee: {employeeName} ({employeeType})");
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
                                        header.Cell().Element(CellStyle).Text("Product").Bold();
                                        header.Cell().Element(CellStyle).Text("Qty").Bold();
                                        if (type.ToLower() == "customer")
                                            header.Cell().Element(CellStyle).Text("Price").Bold();
                                        header.Cell().Element(CellStyle).Text(type.ToLower() == "customer" ? "Total" : "");
                                    });
                                    foreach (var p in tx.TransactionProducts)
                                    {
                                        table.Cell().Element(CellStyle).Text(p.Name);
                                        table.Cell().Element(CellStyle).Text(p.Quantity.ToString());
                                        if (type.ToLower() == "customer")
                                            table.Cell().Element(CellStyle).Text($"Rp. {p.Price:N0}");
                                        table.Cell().Element(CellStyle).Text(type.ToLower() == "customer" ? $"Rp. {(p.Price * p.Quantity):N0}" : "");
                                    }
                                });
                                if (type.ToLower() == "customer")
                                {
                                    var total = tx.TransactionProducts.Sum(x => x.Price * x.Quantity);
                                    col.Item().AlignRight().Text($"Total: Rp. {total:N0}").Bold();
                                }
                                col.Item().PaddingBottom(10);
                            }
                            // Tambahkan garis horizontal dan grand total di bawah transaksi terakhir (khusus customer)
                            if (type.ToLower() == "customer")
                            {
                                col.Item().PaddingVertical(10).BorderBottom(3).BorderColor(Colors.Black);
                                col.Item().AlignRight().Text($"Grand Total: Rp. {grandTotal:N0}").FontSize(14).Bold();
                            }
                        });
                    });
                });
            });

            static IContainer CellStyle(IContainer container) =>
                container.Border(1).BorderColor(Colors.Grey.Darken2).PaddingVertical(2).PaddingHorizontal(4);

            return document.GeneratePdf();
        }

        [HttpGet("receipt")]
        [Obsolete]
        public async Task<IActionResult> DownloadReceipt([FromQuery] int transactionId)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var transaction = await db.Transactions
                    .Include(x => x.TransactionProducts)
                    .Include(x => x.ProcessedBy)
                    .Include(x => x.Employee)
                    .FirstOrDefaultAsync(x => x.Id == transactionId);
                if (transaction == null)
                {
                    return NotFound("Transaction not found");
                }
                var pdfBytes = GenerateReceiptPdf(transaction);
                var fileName = $"Receipt_{transaction.Id}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating receipt PDF");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Obsolete]
        private byte[] GenerateReceiptPdf(Data.ApplicationDb.Transaction tx)
        {
            // Load logo from wwwroot/images/log.png
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
            byte[]? logoBytes = null;
            if (System.IO.File.Exists(logoPath))
                logoBytes = System.IO.File.ReadAllBytes(logoPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            if (logoBytes != null)
                                col.Item().Height(60).Image(logoBytes, ImageScaling.FitHeight);
                            col.Item().Text("Neo Global School").FontSize(18).Bold();
                            col.Item().Text("Jl. Gn. Bintan No. 15, Kota Batam");
                            col.Item().Text("Telp. 08117773775");
                        });
                        row.ConstantItem(300).Column(col =>
                        {
                            col.Item().AlignRight().Text("BUKTI PEMBAYARAN").FontSize(14).Bold();
                            col.Item().AlignRight().Text($"TRX{tx.Id.ToString().PadLeft(6, '0')}").FontColor(Colors.Red.Medium).FontSize(14).Bold();
                        });
                    });
                    page.Content().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Customer : {(tx.Employee != null ? tx.Employee.Name : tx.CustomerName)}").Bold();
                                c.Item().Text($"Tanggal : {tx.TimeStamp:dd MMM yyyy}");
                                c.Item().Text($"Kasir : {tx.ProcessedBy.Name}");
                            });
                        });
                        col.Item().PaddingVertical(10);
                        col.Item().Text($"Kode Transaksi : TRX{tx.Id.ToString().PadLeft(6, '0')}").Bold();
                        col.Item().Text($"Tanggal : {tx.TimeStamp:dd MMM yyyy HH:mm}");
                        col.Item().PaddingVertical(10);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(); // Product
                                columns.ConstantColumn(60); // Qty
                                columns.ConstantColumn(90); // Total
                            });
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Product").Bold();
                                header.Cell().Element(CellStyle).Text("Qty").Bold();
                                header.Cell().Element(CellStyle).Text("Total").Bold();
                            });
                            foreach (var p in tx.TransactionProducts)
                            {
                                table.Cell().Element(CellStyle).Text(p.Name);
                                table.Cell().Element(CellStyle).Text(p.Quantity.ToString());
                                table.Cell().Element(CellStyle).Text($"Rp. {(p.Price * p.Quantity):N0}");
                            }
                        });
                        col.Item().PaddingVertical(10);
                        var total = tx.TransactionProducts.Sum(x => x.Price * x.Quantity);
                        col.Item().AlignRight().Text($"Total Pembayaran  Rp. {total:N0}").FontSize(14).Bold();
                        col.Item().Text("*Dokumen ini dihasilkan komputer. Tidak perlu tanda tangan.").FontSize(8).Italic();
                    });
                });
            });
            static IContainer CellStyle(IContainer container) =>
                container.Border(1).BorderColor(Colors.Grey.Darken2).PaddingVertical(2).PaddingHorizontal(4);
            return document.GeneratePdf();
        }
    }
}
