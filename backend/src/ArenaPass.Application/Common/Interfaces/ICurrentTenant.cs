namespace ArenaPass.Application.Common.Interfaces;

// Estado do tenant (Espaco) atual da requisição, resolvido pelo middleware de tenant
// (via claim "espacoId" do JWT, pós-autenticação, ou header X-Tenant, pré-autenticação).
// Null para o Master (cross-tenant) ou quando nenhum tenant pôde ser resolvido.
public interface ICurrentTenant
{
    Guid? EspacoId { get; set; }
}
