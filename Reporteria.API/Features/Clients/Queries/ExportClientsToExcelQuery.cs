using MediatR;
using ClosedXML.Excel;
using Reporteria.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Reporteria.API.Features.Clients.Queries;
public record ExportClientsToExcelQuery() : IRequest<byte[]>;

public class ExportClientsToExcelQueryHandler : IRequestHandler<ExportClientsToExcelQuery, byte[]>
{
    private readonly ApplicationDbContext _context;

    public ExportClientsToExcelQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> Handle(ExportClientsToExcelQuery request, CancellationToken cancellationToken)
    {
        // 1. Obtenemos los datos de la base de datos
        var clients = await _context.Clients.AsNoTracking().ToListAsync(cancellationToken);

        // 2. Creamos un libro de trabajo de Excel usando ClosedXML 
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Clientes");

            // 3. Creamos la cabecera y aplicar estilos (Parte 5 de la guía) 
            var headerRow = worksheet.Row(1);
            worksheet.Cell(1, 1).Value = "ID Cliente";
            worksheet.Cell(1, 2).Value = "Nombre Completo";
            worksheet.Cell(1, 3).Value = "Email";
            
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            headerRow.Style.Font.FontColor = XLColor.White;

            // 4. Insertamos los datos de los clientes en la hoja
            worksheet.Cell(2, 1).InsertData(clients);

            // 5. Creamos una tabla formateada (Parte 4 de la guía) 
            var range = worksheet.RangeUsed();
            var table = range.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium2;

            // Ajustamos el ancho de las columnas al contenido
            worksheet.Columns().AdjustToContents();

            // 6. Guardamos el archivo para enviarlo en la respuesta
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}