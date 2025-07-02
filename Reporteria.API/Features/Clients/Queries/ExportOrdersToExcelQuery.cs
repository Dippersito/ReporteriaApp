using MediatR;
using ClosedXML.Excel;
using Reporteria.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Reporteria.API.Features.Orders.Queries;

public record ExportOrdersToExcelQuery() : IRequest<byte[]>;

public class ExportOrdersToExcelQueryHandler : IRequestHandler<ExportOrdersToExcelQuery, byte[]>
{
    private readonly ApplicationDbContext _context;

    public ExportOrdersToExcelQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> Handle(ExportOrdersToExcelQuery request, CancellationToken cancellationToken)
    {
        // 1. Obtenemos los datos usando Include() para traer las relaciones
        var orders = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .AsNoTracking()
            .OrderBy(o => o.OrderDate)
            .ToListAsync(cancellationToken);
            
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Detalle de Órdenes");
            var currentRow = 1;

            // 2. Definir la cabecera
            worksheet.Cell(currentRow, 1).Value = "ID Orden";
            worksheet.Cell(currentRow, 2).Value = "Fecha";
            worksheet.Cell(currentRow, 3).Value = "Cliente";
            worksheet.Cell(currentRow, 4).Value = "Producto";
            worksheet.Cell(currentRow, 5).Value = "Cantidad";
            worksheet.Cell(currentRow, 6).Value = "Precio Unitario";
            worksheet.Cell(currentRow, 7).Value = "Subtotal";
            
            // Aplicar estilo a la cabecera
            var headerRow = worksheet.Row(currentRow);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.DarkSeaGreen;

            // 3. Llenar los datos recorriendo las órdenes y sus detalles
            foreach (var order in orders)
            {
                foreach (var detail in order.OrderDetails)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = order.OrderId;
                    worksheet.Cell(currentRow, 2).Value = order.OrderDate;
                    worksheet.Cell(currentRow, 2).Style.DateFormat.Format = "dd-MMM-yyyy";
                    worksheet.Cell(currentRow, 3).Value = order.Client.Name;
                    worksheet.Cell(currentRow, 4).Value = detail.Product.Name;
                    worksheet.Cell(currentRow, 5).Value = detail.Quantity;
                    worksheet.Cell(currentRow, 6).Value = detail.Product.Price;
                    worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "$ #,##0.00";
                    
                    // Usar una fórmula de Excel para el subtotal
                    worksheet.Cell(currentRow, 7).FormulaA1 = $"=E{currentRow}*F{currentRow}";
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "$ #,##0.00";
                }
            }
            
            worksheet.Columns().AdjustToContents();

            // 4. Guardar el archivo en memoria y devolverlo
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}