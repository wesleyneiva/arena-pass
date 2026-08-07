using MediatR;

namespace ArenaPass.Application.Espacos.Queries.ResolverEspaco;

public record ResolverEspacoQuery : IRequest<ResolverEspacoResult>;

public record ResolverEspacoResult(bool Encontrado, string? Nome);
