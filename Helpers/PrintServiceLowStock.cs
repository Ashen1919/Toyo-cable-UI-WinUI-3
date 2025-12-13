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
    public class PrintServiceLowStock
    {
        public PrintServiceLowStock()
        {
            // Set QuestPDF license (use Community for non-commercial)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateLowStockReportPdf(List<LowStockProduct> lowStockProducts)
        {
            // Separate products by stock level
            var criticalProducts = lowStockProducts.Where(p => p.StockLevel == StockLevel.Critical).ToList();
            var lowProducts = lowStockProducts.Where(p => p.StockLevel == StockLevel.Low).ToList();

            // Generate PDF in memory first
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, criticalProducts, lowProducts));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();

            // Save to temp folder
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var fileName = $"LowStock_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var file = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteBytesAsync(file, pdfBytes);

            return file.Path;
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("TOYO CABLE").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Low Stock Alert Report").FontSize(16).FontColor(Colors.Red.Darken1).Bold();
                    column.Item().PaddingTop(5).Text(txt =>
                    {
                        txt.Span("Generated: ").SemiBold();
                        txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });

                row.ConstantItem(100).Height(60).Image(GetLogoBytes());
            });
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

        private void ComposeContent(IContainer container, List<LowStockProduct> criticalProducts, List<LowStockProduct> lowProducts)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(20);

                // Summary Section
                column.Item().Element(c => ComposeSummary(c, criticalProducts.Count, lowProducts.Count));

                // Critical Stock Section
                if (criticalProducts.Any())
                {
                    column.Item().Element(c => ComposeCriticalSection(c, criticalProducts));
                }

                // Low Stock Section
                if (lowProducts.Any())
                {
                    column.Item().Element(c => ComposeLowStockSection(c, lowProducts));
                }
            });
        }

        private void ComposeSummary(IContainer container, int criticalCount, int lowStockCount)
        {
            var totalItems = criticalCount + lowStockCount;

            container.Background(Colors.Grey.Lighten3).Padding(15).Column(column =>
            {
                column.Spacing(10);

                column.Item().Text("Stock Alert Summary").FontSize(16).Bold();

                column.Item().Row(row =>
                {
                    row.RelativeItem().Background(Colors.Red.Lighten4).Padding(10).Column(col =>
                    {
                        col.Item().Text("CRITICAL STOCK").FontSize(10).SemiBold().FontColor(Colors.Red.Darken2);
                        col.Item().Text(criticalCount.ToString()).FontSize(24).Bold().FontColor(Colors.Red.Darken2);
                        col.Item().Text("Below 10 units").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    row.Spacing(10);

                    row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(col =>
                    {
                        col.Item().Text("LOW STOCK").FontSize(10).SemiBold().FontColor(Colors.Orange.Darken2);
                        col.Item().Text(lowStockCount.ToString()).FontSize(24).Bold().FontColor(Colors.Orange.Darken2);
                        col.Item().Text("10-50 units").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    row.Spacing(10);

                    row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
                    {
                        col.Item().Text("TOTAL ITEMS").FontSize(10).SemiBold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(totalItems.ToString()).FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Need attention").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }

        private void ComposeCriticalSection(IContainer container, List<LowStockProduct> criticalProducts)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                // Section Header
                column.Item().Background(Colors.Red.Darken2).Padding(8).Row(row =>
                {
                    row.RelativeItem().Text("⚠️ CRITICAL STOCK - IMMEDIATE ACTION REQUIRED")
                        .FontSize(12).Bold().FontColor(Colors.White);
                    row.ConstantItem(80).AlignRight().Text($"{criticalProducts.Count} items")
                        .FontSize(10).SemiBold().FontColor(Colors.White);
                });

                // Products Table
                column.Item().Element(c => ComposeProductsTable(c, criticalProducts, Colors.Red.Lighten4));
            });
        }

        private void ComposeLowStockSection(IContainer container, List<LowStockProduct> lowProducts)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                // Section Header
                column.Item().Background(Colors.Orange.Darken2).Padding(8).Row(row =>
                {
                    row.RelativeItem().Text("📦 LOW STOCK - REORDER RECOMMENDED")
                        .FontSize(12).Bold().FontColor(Colors.White);
                    row.ConstantItem(80).AlignRight().Text($"{lowProducts.Count} items")
                        .FontSize(10).SemiBold().FontColor(Colors.White);
                });

                // Products Table
                column.Item().Element(c => ComposeProductsTable(c, lowProducts, Colors.Orange.Lighten4));
            });
        }

        private void ComposeProductsTable(IContainer container, List<LowStockProduct> products, string highlightColor)
        {
            container.Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // #
                    columns.RelativeColumn(3);    // Product Name
                    columns.RelativeColumn(2);    // Category
                    columns.RelativeColumn(1);    // Stock
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("#");
                    header.Cell().Element(HeaderCellStyle).Text("Product Name");
                    header.Cell().Element(HeaderCellStyle).Text("Category");
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Current Stock");

                    static IContainer HeaderCellStyle(IContainer c) => c
                        .Background(Colors.Grey.Darken2)
                        .Padding(8)
                        .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White));
                });

                // Items
                int index = 1;
                foreach (var product in products)
                {
                    var bgColor = index % 2 == 0 ? Color.FromHex(highlightColor) : Colors.White;

                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(index.ToString());
                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(product.ProductName);
                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(product.CategoryName);
                    table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter()
                        .Text($"{product.CurrentStock} units")
                        .FontColor(product.StockLevel == StockLevel.Critical ? Colors.Red.Darken2 : Colors.Orange.Darken2)
                        .Bold();

                    index++;
                }

                static IContainer CellStyle(IContainer c, string color) => c
                    .Background(color)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(8);
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Column(column =>
            {
                column.Spacing(5);

                column.Item().Text(txt =>
                {
                    txt.Span("This report is confidential and intended for internal use only. | ");
                    txt.Span("Generated by TOYO CABLE Inventory System");
                });

                column.Item().Text(txt =>
                {
                    txt.Span("Page ").FontSize(9);
                    txt.CurrentPageNumber().FontSize(9);
                    txt.Span(" of ").FontSize(9);
                    txt.TotalPages().FontSize(9);
                });
            });
        }

        public async Task OpenPdfAsync(string filePath)
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            await Launcher.LaunchFileAsync(file);
        }

        // Alternative method name for consistency
        public async Task<string> PrintReportAsync(List<LowStockProduct> lowStockProducts)
        {
            return await GenerateLowStockReportPdf(lowStockProducts);
        }
    }
}