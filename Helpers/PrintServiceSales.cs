using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Toyo_cable_UI.Models;
using Windows.Storage;
using Windows.System;

namespace Toyo_cable_UI.Services
{
    public class PrintServiceSales
    {
        public PrintServiceSales()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // Generate Daily Sales Report
        public async Task<string> GenerateDailySalesReportPdf(
            DateTime reportDate,
            List<DailySalesData> salesData,
            int totalOrders,
            decimal totalRevenue,
            decimal avgRevenue,
            int totalItemsSold)
        {
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Header().Element(header => ComposeDailyReportHeader(header, reportDate));
                    page.Content().Element(content => ComposeDailyReportContent(
                        content,
                        salesData,
                        totalOrders,
                        totalRevenue,
                        avgRevenue,
                        totalItemsSold));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();

            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var fileName = $"DailySalesReport_{reportDate:yyyyMMdd}_{DateTime.Now:HHmmss}.pdf";
            var file = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteBytesAsync(file, pdfBytes);

            return file.Path;
        }

        private void ComposeDailyReportHeader(IContainer container, DateTime reportDate)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("TOYO CABLE").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Daily Sales Report").FontSize(16).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).Text(txt =>
                    {
                        txt.Span("Report Date: ").SemiBold();
                        txt.Span(reportDate.ToString("dd MMMM yyyy"));
                    });
                    column.Item().Text(txt =>
                    {
                        txt.Span("Generated: ").FontSize(9).FontColor(Colors.Grey.Medium);
                        txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });

                row.ConstantItem(100).Height(60).Image(GetLogoBytes());
            });
        }

        private void ComposeDailyReportContent(
            IContainer container,
            List<DailySalesData> salesData,
            int totalOrders,
            decimal totalRevenue,
            decimal avgRevenue,
            int totalItemsSold)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(20);

                // Summary Cards
                column.Item().Element(c => ComposeSummarySection(c, totalOrders, totalRevenue, avgRevenue, totalItemsSold));

                // Sales Data Table
                column.Item().Element(c => ComposeSalesDataTable(c, salesData));

                // Footer Note
                column.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10)
                    .Text("This report contains confidential information for internal use only.")
                    .FontSize(8)
                    .Italic()
                    .FontColor(Colors.Grey.Medium);
            });
        }

        private void ComposeSummarySection(
            IContainer container,
            int totalOrders,
            decimal totalRevenue,
            decimal avgRevenue,
            int totalItemsSold)
        {
            container.Background(Colors.Blue.Lighten4).Padding(15).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text("Daily Summary").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                column.Item().Row(row =>
                {
                    row.Spacing(20);

                    // Total Orders
                    row.RelativeItem().Background(Colors.White).Padding(12).Column(col =>
                    {
                        col.Item().Text("Total Orders").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(totalOrders.ToString()).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    });

                    // Total Revenue
                    row.RelativeItem().Background(Colors.White).Padding(12).Column(col =>
                    {
                        col.Item().Text("Total Revenue").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"Rs. {totalRevenue:N2}").FontSize(15).Bold().FontColor(Colors.Green.Darken2);
                    });

                    // Average Revenue
                    row.RelativeItem().Background(Colors.White).Padding(12).Column(col =>
                    {
                        col.Item().Text("Avg Order Value").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"Rs. {avgRevenue:N2}").FontSize(15).Bold().FontColor(Colors.Orange.Darken1);
                    });

                    // Total Items Sold
                    row.RelativeItem().Background(Colors.White).Padding(12).Column(col =>
                    {
                        col.Item().Text("Items Sold").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(totalItemsSold.ToString()).FontSize(20).Bold().FontColor(Colors.Purple.Darken1);
                    });
                });
            });
        }

        private void ComposeSalesDataTable(IContainer container, List<DailySalesData> salesData)
        {
            container.Column(column =>
            {
                column.Spacing(10);
                column.Item().Text("Top Selling Products").FontSize(14).Bold();

                if (salesData == null || !salesData.Any())
                {
                    column.Item().Padding(20).AlignCenter().Text("No sales data available")
                        .FontSize(12)
                        .FontColor(Colors.Grey.Medium);
                    return;
                }

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);    // Rank
                        columns.RelativeColumn(3);      // Product Name
                        columns.RelativeColumn(1);      // Qty Sold
                        columns.RelativeColumn(1.5f);   // Revenue
                        columns.RelativeColumn(1);      // Orders
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("Rank");
                        header.Cell().Element(HeaderCellStyle).Text("Product Name");
                        header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Qty Sold");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Revenue (Rs.)");
                        header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Orders");

                        static IContainer HeaderCellStyle(IContainer c) => c
                            .Background(Colors.Blue.Darken2)
                            .Padding(8)
                            .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White));
                    });

                    // Data Rows
                    int rank = 1;
                    foreach (var item in salesData)
                    {
                        var bgColor = rank % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Element(c => CellStyle(c, bgColor)).Text(rank.ToString()).SemiBold();
                        table.Cell().Element(c => CellStyle(c, bgColor)).Text(item.ProductName);
                        table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(item.TotalQuantitySold.ToString());
                        table.Cell().Element(c => CellStyle(c, bgColor)).AlignRight().Text($"{item.TotalRevenue:N2}").FontColor(Colors.Green.Darken2);
                        table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(item.OrderCount.ToString());

                        rank++;
                    }

                    // Total Row
                    var totalQty = salesData.Sum(s => s.TotalQuantitySold);
                    var totalRev = salesData.Sum(s => s.TotalRevenue);

                    table.Cell().ColumnSpan(2).Element(TotalCellStyle).Text("TOTAL").Bold();
                    table.Cell().Element(TotalCellStyle).AlignCenter().Text(totalQty.ToString()).Bold();
                    table.Cell().Element(TotalCellStyle).AlignRight().Text($"{totalRev:N2}").Bold().FontColor(Colors.Green.Darken2);
                    table.Cell().Element(TotalCellStyle);

                    static IContainer CellStyle(IContainer c, string color) => c
                        .Background(color)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(8);

                    static IContainer TotalCellStyle(IContainer c) => c
                        .Background(Colors.Blue.Lighten3)
                        .BorderTop(2)
                        .BorderColor(Colors.Blue.Darken2)
                        .Padding(8);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(txt =>
            {
                txt.Span("Toyo Cable - Confidential Report | ");
                txt.Span("Page ").FontSize(9);
                txt.CurrentPageNumber().FontSize(9);
                txt.Span(" of ").FontSize(9);
                txt.TotalPages().FontSize(9);
            });
        }


        public async Task<string> DailySalesReportPdf(
            DateTime reportDate,
            List<DailySalesData> salesData,
            int totalOrders,
            decimal totalRevenue,
            decimal avgRevenue,
            int totalItemsSold)
        {
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    // Header
                    page.Header().Element(header => ComposeDailyReportHeader(header, reportDate));

                    // Content - proper lambda syntax
                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(20);

                        // Summary Cards
                        column.Item().Element(c => ComposeSummarySection(c, totalOrders, totalRevenue, avgRevenue, totalItemsSold));

                        // Sales Data Table
                        column.Item().Element(c => ComposeSalesDataTable(c, salesData));

                        // Footer Note
                        column.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10)
                            .Text("This report contains confidential information for internal use only.")
                            .FontSize(8)
                            .Italic()
                            .FontColor(Colors.Grey.Medium);
                    });

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();

            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var fileName = $"DailySalesReport_{reportDate:yyyyMMdd}_{DateTime.Now:HHmmss}.pdf";
            var file = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteBytesAsync(file, pdfBytes);

            return file.Path;
        }
        private byte[] GetLogoBytes()
        {
            try
            {
                var logoPath = Path.Combine(
                    Windows.ApplicationModel.Package.Current.InstalledLocation.Path,
                    "Assets",
                    "Square150x150Logo.scale-200.png"
                );

                if (File.Exists(logoPath))
                {
                    return File.ReadAllBytes(logoPath);
                }

                return CreatePlaceholderImage();
            }
            catch
            {
                return CreatePlaceholderImage();
            }
        }

        private byte[] CreatePlaceholderImage()
        {
            return new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
                0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                0x42, 0x60, 0x82
            };
        }

        public async Task OpenPdfAsync(string filePath)
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            await Launcher.LaunchFileAsync(file);
        }

    }
}