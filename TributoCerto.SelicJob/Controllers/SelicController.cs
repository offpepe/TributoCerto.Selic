using Microsoft.AspNetCore.Mvc;
using TributoCerto.Selic.Dtos;
using TributoCerto.Selic.Service.Interfaces;

namespace TributoCerto.Selic.Controllers;

[Route("[controller]")]
public class SelicController : ControllerBase
{
    private readonly ISelicService _service;

    public SelicController(ISelicService service)
    {
        _service = service;
    }
    [HttpGet]
    public async Task<IList<SelicAccumulatedDto>> GetAllTaxes()
    {
        return await _service.SearchSelicTax();
    }
    [HttpPut("[action]")]
    public async Task UpdateSelicTaxes()
    {
        await _service.SeedDatabaseWithSelicTaxes(DateTime.Now.Year - 2013);
    }
}