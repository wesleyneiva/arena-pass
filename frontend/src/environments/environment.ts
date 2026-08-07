export const environment = {
  production: true,
  apiUrl: 'https://api-arenapass.wnlabs.com.br/api',
  // Domínio base do wildcard de subdomínios (*.wnlabs.com.br) — cada cliente vira um
  // subdomínio aqui (ex: hrtennis.wnlabs.com.br). Requer o domínio wnlabs.com.br
  // apontado/adicionado no projeto da Vercel para cada subdomínio usado.
  baseDomain: 'wnlabs.com.br' as string | undefined,
  tenantPadrao: undefined as string | undefined
};
