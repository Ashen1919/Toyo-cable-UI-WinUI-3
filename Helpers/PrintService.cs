using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Toyo_cable_UI.Models;
using Windows.Storage;
using Windows.System;

namespace Toyo_cable_UI.Services
{
    public class PrintService
    {
        public PrintService()
        {
            // Set QuestPDF license (use Community for non-commercial)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateOrderPdf(Order order, List<OrderItems> orderItems)
        {
            // Generate PDF in memory first
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, order, orderItems));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();

            // Save to temp folder
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var fileName = $"Invoice_{order.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
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
                    column.Item().Text("Sales Order Invoice").FontSize(14).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).Text(txt =>
                    {
                        txt.Span("Date: ").SemiBold();
                        txt.Span(DateTime.Now.ToString("dd/MM/yyyy"));
                    });
                });

                row.ConstantItem(100).Height(60).Image(GetLogoBytes());
            });
        }
        private byte[] GetLogoBytes()
        {
            try
            {
                // Load from Assets folder
                // Change "logo.png" to your actual logo filename
                var logoPath = Path.Combine(
                    Windows.ApplicationModel.Package.Current.InstalledLocation.Path,
                    "Assets",
                    "Square150x150Logo.scale-200.png"
                );

                if (File.Exists(logoPath))
                {
                    return File.ReadAllBytes(logoPath);
                }

                // If logo not found, return placeholder
                return CreatePlaceholderImage();
            }
            catch
            {
                // Return placeholder on error
                return CreatePlaceholderImage();
            }
        }

        private byte[] CreatePlaceholderImage()
        {
            // Create a simple 1x1 transparent PNG as placeholder
            // This is a minimal valid PNG file
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
        private void ComposeContent(IContainer container, Order order, List<OrderItems> orderItems)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(15);

                // Order Information Section
                column.Item().Element(c => ComposeOrderInfo(c, order));

                // Items Table
                column.Item().Element(c => ComposeItemsTable(c, orderItems));

                // Totals Section
                column.Item().Element(c => ComposeTotals(c, order));
            });
        }

        private void ComposeOrderInfo(IContainer container, Order order)
        {
            container.Background(Colors.Grey.Lighten3).Padding(15).Column(column =>
            {
                column.Spacing(5);

                column.Item().Text("Order Information").FontSize(14).Bold();

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(txt =>
                    {
                        txt.Span("Order ID: ").SemiBold();
                        txt.Span(order.Id.ToString());
                    });

                    row.RelativeItem().Text(txt =>
                    {
                        txt.Span("Order Date: ").SemiBold();
                        txt.Span(order.OrderTime.ToString("dd/MM/yyyy HH:mm") ?? "N/A");
                    });
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(txt =>
                    {
                        txt.Span("Status: ").SemiBold();
                        txt.Span("Completed").FontColor(Colors.Green.Darken2);
                    });
                });
            });
        }

        private void ComposeItemsTable(IContainer container, List<OrderItems> items)
        {
            container.Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40); // #
                    columns.RelativeColumn(3);   // Product Name
                    columns.RelativeColumn(1);   // Quantity
                    columns.RelativeColumn(1.5f); // Unit Price
                    columns.RelativeColumn(1.5f); // Total
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("#");
                    header.Cell().Element(HeaderCellStyle).Text("Product Name");
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Quantity");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Unit Price (Rs.)");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total (Rs.)");

                    static IContainer HeaderCellStyle(IContainer c) => c
                        .Background(Colors.Blue.Darken2)
                        .Padding(8)
                        .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White));
                });

                // Items
                int index = 1;
                foreach (var item in items)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(index.ToString());
                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(item.ProductName);
                    table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(item.Quantity.ToString());
                    table.Cell().Element(c => CellStyle(c, bgColor)).AlignRight().Text($"{item.UnitPrice:N2}");
                    table.Cell().Element(c => CellStyle(c, bgColor)).AlignRight().Text($"{item.TotalPrice:N2}");

                    index++;
                }

                static IContainer CellStyle(IContainer c, string color) => c
                    .Background(color)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(8);
            });
        }

        private void ComposeTotals(IContainer container, Order order)
        {
            container.AlignRight().Width(250).Column(column =>
            {
                column.Spacing(8);

                column.Item().Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:").SemiBold();
                    row.ConstantItem(100).AlignRight().Text($"Rs. {order.SubTotal:N2}");
                });

                column.Item().Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                {
                    row.RelativeItem().Text("Discount:").SemiBold().FontColor(Colors.Red.Darken1);
                    row.ConstantItem(100).AlignRight().Text($"- Rs. {order.Discount:N2}").FontColor(Colors.Red.Darken1);
                });

                column.Item().Background(Colors.Blue.Darken2).Padding(10).Row(row =>
                {
                    row.RelativeItem().Text("Total Amount:").Bold().FontColor(Colors.White);
                    row.ConstantItem(100).AlignRight().Text($"Rs. {order.TotalAmount:N2}").Bold().FontColor(Colors.White);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(txt =>
            {
                txt.Span("Thank you for your business! | ");
                txt.Span("Page ").FontSize(9);
                txt.CurrentPageNumber().FontSize(9);
                txt.Span(" of ").FontSize(9);
                txt.TotalPages().FontSize(9);
            });
        }

        public async Task OpenPdfAsync(string filePath)
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            await Launcher.LaunchFileAsync(file);
        }

        // Alias method if you prefer this name
        public async Task<string> PrintReportAsync(Order order, List<OrderItems> orderItems)
        {
            return await GenerateOrderPdf(order, orderItems);
        }
    }
}