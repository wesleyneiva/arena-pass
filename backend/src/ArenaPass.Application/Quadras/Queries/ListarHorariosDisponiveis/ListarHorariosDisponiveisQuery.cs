using ArenaPass.Application.Quadras.Dtos;
using MediatR;

namespace ArenaPass.Application.Quadras.Queries.ListarHorariosDisponiveis;

public record ListarHorariosDisponiveisQuery(Guid QuadraId, DateOnly Data) : IRequest<List<HorarioSlotDto>>;
