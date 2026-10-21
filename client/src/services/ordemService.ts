import type { CriarOrdemRequest, CriarOrdemResponse, ErroApi, FalhaEnvio } from '../types/ordem';

const urlBaseApi = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '');

const lerErroApi = (conteudo: unknown): ErroApi => {
  if (typeof conteudo !== 'object' || conteudo === null) {
    return {};
  }

  return conteudo as ErroApi;
};

const extrairErrosValidacao = (erros?: Record<string, string[]>): string[] =>
  Object.values(erros ?? {}).flat();

const criarFalha = (status: number, erro: ErroApi): FalhaEnvio => {
  const errosValidacao = extrairErrosValidacao(erro.errors);

  if (status === 400) {
    return {
      status,
      titulo: 'Confira os dados da ordem',
      detalhe:
        errosValidacao.length > 0
          ? 'A API encontrou dados inválidos.'
          : (erro.detail ?? 'A API não aceitou os dados enviados.'),
      identificadorOrdem: erro.identificadorOrdem,
      errosValidacao,
    };
  }

  if (status === 503) {
    return {
      status,
      titulo: 'Sessão FIX indisponível',
      detalhe:
        'A API não conseguiu enviar a ordem ao serviço FIX. O envio não foi confirmado. Verifique se o OrderAccumulator.Worker está ativo antes de tentar novamente.',
      identificadorOrdem: erro.identificadorOrdem,
    };
  }

  if (status === 504) {
    return {
      status,
      titulo: 'Resultado da ordem não confirmado',
      detalhe:
        'A API excedeu o tempo de espera por uma resposta FIX. O resultado pode ser desconhecido; confira o processamento antes de enviar novamente.',
      identificadorOrdem: erro.identificadorOrdem,
    };
  }

  return {
    status,
    titulo:
      status === 500
        ? 'Falha ao processar a ordem'
        : (erro.title ?? 'Não foi possível enviar a ordem'),
    detalhe:
      erro.detail ?? 'A API encontrou uma falha técnica. O resultado da ordem não foi confirmado.',
    identificadorOrdem: erro.identificadorOrdem,
  };
};

export const enviarOrdem = async (
  requisicao: CriarOrdemRequest,
): Promise<{ resposta?: CriarOrdemResponse; falha?: FalhaEnvio }> => {
  if (!urlBaseApi) {
    return {
      falha: {
        status: null,
        titulo: 'API não configurada',
        detalhe: 'Defina VITE_API_BASE_URL no arquivo .env.local e reinicie o frontend.',
      },
    };
  }

  let resposta: Response;

  try {
    resposta = await fetch(`${urlBaseApi}/api/ordens`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(requisicao),
    });
  } catch {
    return {
      falha: {
        status: null,
        titulo: 'Não foi possível comunicar com a API',
        detalhe:
          'A conexão falhou antes da confirmação do envio. O resultado da ordem é desconhecido; verifique o serviço antes de tentar novamente.',
      },
    };
  }

  let conteudo: unknown;

  try {
    conteudo = await resposta.json();
  } catch {
    conteudo = {};
  }

  if (!resposta.ok) {
    return { falha: criarFalha(resposta.status, lerErroApi(conteudo)) };
  }

  return { resposta: conteudo as CriarOrdemResponse };
};
