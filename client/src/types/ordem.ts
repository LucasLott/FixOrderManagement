export type LadoOrdem = 'Compra' | 'Venda';

export interface CriarOrdemRequest {
  simbolo: string;
  lado: LadoOrdem;
  quantidade: number;
  preco: number;
}

export interface CriarOrdemResponse {
  identificadorOrdem: string;
  simbolo: string;
  aceita: boolean;
  motivoRejeicao: string | null;
  identificadorExecucao: string | null;
  identificadorOrdemServidor: string | null;
}

export interface ErroApi {
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  identificadorOrdem?: string;
}

export interface FalhaEnvio {
  status: number | null;
  titulo: string;
  detalhe: string;
  identificadorOrdem?: string;
  errosValidacao?: string[];
}

export type ResultadoEnvio =
  { tipo: 'resposta'; ordem: CriarOrdemResponse } | { tipo: 'falha'; falha: FalhaEnvio };
