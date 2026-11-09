using CoMind.Application.Common.Interfaces;
using CoMind.Application.Common.Models;
using CoMind.Application.Common.Security;
using CoMind.Domain.Enums;
using CoMind.Domain.ValueObjects;

namespace CoMind.Application.TodoLists.Queries.GetTodos;

[Authorize]
public record GetTodosQuery : IRequest<TodosVm>;

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, TodosVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTodosQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        return new TodosVm
        {
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                .Cast<PriorityLevel>()
                .Select(p => new LookupDto { Id = (int)p, Title = p.ToString() })
                .ToList(),

            Colours =
            [
                new ColourDto { Code = Color.Grey, Name = nameof(Color.Grey) },
                new ColourDto { Code = Color.Purple, Name = nameof(Color.Purple) },
                new ColourDto { Code = Color.Blue, Name = nameof(Color.Blue) },
                new ColourDto { Code = Color.Teal, Name = nameof(Color.Teal) },
                new ColourDto { Code = Color.Green, Name = nameof(Color.Green) },
                new ColourDto { Code = Color.Orange, Name = nameof(Color.Orange) },
                new ColourDto { Code = Color.Red, Name = nameof(Color.Red) },
            ],

            Lists = await _context.TodoLists
                .AsNoTracking()
                .ProjectTo<TodoListDto>(_mapper.ConfigurationProvider)
                .OrderBy(t => t.Title)
                .ToListAsync(cancellationToken)
        };
    }
}
