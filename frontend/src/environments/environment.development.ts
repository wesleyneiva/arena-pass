export const environment = {
  production: false,
  apiUrl: 'https://localhost:7074/api',
  baseDomain: undefined as string | undefined,
  // localhost não tem subdomínio — usa o mesmo espaço seedado localmente pelo backend
  // (ver EspacoPadraoSubdominio em ArenaPassDbContextSeed) como padrão de dev.
  // Pode ser sobrescrito via ?tenant=outro-slug pra testar múltiplos espaços localmente.
  tenantPadrao: 'hrtennis' as string | undefined
};
