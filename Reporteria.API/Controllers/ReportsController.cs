using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reporteria.API.Features.Clients.Queries;
using Reporteria.API.Features.Orders.Queries;

namespace Reporteria.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("clients")]
    public async Task<IActionResult> GetClientsReport()
    {
        var query = new ExportClientsToExcelQuery();
        var fileContents = await _mediator.Send(query);
        
        // Devolvemos el archivo para que el navegador inicie la descarga.
        return File(fileContents, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "Reporte_Clientes.xlsx");
    }
    
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrdersReport()
    {
        var query = new ExportOrdersToExcelQuery();
        var fileContents = await _mediator.Send(query);
        
        return File(fileContents, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "Reporte_Detallado_Ordenes.xlsx");
    }
}