using RestSharp;
using TributoCerto.Selic.Data.Interfaces;
using TributoCerto.Selic.Dtos;

namespace TributoCerto.Selic.Data;

public class SelicApi : ISelicApi
{
    private readonly RestClient _client;

    public SelicApi(IConfiguration configuration)
    {
        _client = new RestClient(configuration.GetValue<string>("Selic"));
    }
    
    public async Task<IList<SelicDto>> GetSelicTax(DateTime startDate, DateTime endDate)
    {
        var req = new RestRequest();
        req.AddQueryParameter("formato", "json");
        req.AddQueryParameter("dataInicial", startDate.ToString("dd/MM/yyyy"));
        req.AddQueryParameter("dataFinal", endDate.ToString("dd/MM/yyyy"));
        return await _client.GetAsync<IList<SelicDto>>(req) ?? new List<SelicDto>();
    }
}