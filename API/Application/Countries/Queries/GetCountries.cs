using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using MediatR;

namespace LeavePlanner.Application.Countries.Queries;

public record GetCountriesQuery : IQuery<Result<List<Country>>>;

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<List<Country>>>
{
	private readonly ICountryRepository _countries;

	public GetCountriesQueryHandler(ICountryRepository countries) => _countries = countries;

	public async Task<Result<List<Country>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken) =>
		Result<List<Country>>.Success(await _countries.GetAllAsync(cancellationToken));
}
