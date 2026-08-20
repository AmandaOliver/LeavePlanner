using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Countries.Queries;

public record GetCountriesQuery : IQuery<Result<List<Country>>>;

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<List<Country>>>
{
	private readonly LeavePlannerContext _context;

	public GetCountriesQueryHandler(LeavePlannerContext context) => _context = context;

	public async Task<Result<List<Country>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
	{
		var countries = await _context.Countries.ToListAsync(cancellationToken);
		return Result<List<Country>>.Success(countries);
	}
}
