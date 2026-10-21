import type { ResultadoEnvio } from '../types/ordem';

interface PropriedadesResultado {
  resultado: ResultadoEnvio | null;
}

export const ResultadoOrdem = ({ resultado }: PropriedadesResultado) => {
  if (!resultado) {
    return (
      <section className="cartaoResultado resultadoVazio" aria-labelledby="titulo-resultado">
        <div className="iconeResultado" aria-hidden="true">
          ↗
        </div>
        <p className="sobretitulo">Retorno da API</p>
        <h2 id="titulo-resultado">Aguardando uma ordem</h2>
        <p>O status do processamento e os identificadores retornados aparecerão aqui.</p>
        <div className="linhaStatus">
          <span className="pontoStatus" /> Nenhum envio realizado
        </div>
      </section>
    );
  }

  if (resultado.tipo === 'falha') {
    const { falha } = resultado;

    return (
      <section
        className="cartaoResultado resultadoFalha"
        aria-labelledby="titulo-resultado"
        role="alert"
      >
        <div className="iconeResultado" aria-hidden="true">
          !
        </div>
        <p className="sobretitulo">Falha técnica{falha.status ? ` · HTTP ${falha.status}` : ''}</p>
        <h2 id="titulo-resultado">{falha.titulo}</h2>
        <p>{falha.detalhe}</p>
        {falha.errosValidacao && falha.errosValidacao.length > 0 && (
          <ul className="listaErros">
            {falha.errosValidacao.map((erro, indice) => (
              <li key={`${indice}-${erro}`}>{erro}</li>
            ))}
          </ul>
        )}
        {falha.identificadorOrdem && (
          <div className="dadoResultado">
            <span>Identificador da ordem</span>
            <code>{falha.identificadorOrdem}</code>
          </div>
        )}
        <div className="avisoResultado">Nenhuma nova tentativa foi feita automaticamente.</div>
      </section>
    );
  }

  const ordem = resultado.ordem;
  const aceita = ordem.aceita;

  return (
    <section
      className={`cartaoResultado ${aceita ? 'resultadoAceita' : 'resultadoRejeitada'}`}
      aria-labelledby="titulo-resultado"
      role="status"
    >
      <div className="iconeResultado" aria-hidden="true">
        {aceita ? '✓' : '×'}
      </div>
      <p className="sobretitulo">Resposta do Accumulator</p>
      <h2 id="titulo-resultado">Ordem {aceita ? 'aceita' : 'rejeitada'}</h2>
      <p>
        {aceita
          ? 'O processamento da ordem foi confirmado pelo serviço FIX.'
          : 'A solicitação foi processada, mas rejeitada por uma regra de negócio.'}
      </p>
      <dl className="dadosResultado">
        <div className="dadoResultado">
          <dt>Identificador da ordem</dt>
          <dd>
            <code>{ordem.identificadorOrdem}</code>
          </dd>
        </div>
        <div className="dadoResultado">
          <dt>Símbolo</dt>
          <dd>{ordem.simbolo}</dd>
        </div>
        {aceita && ordem.identificadorExecucao && (
          <div className="dadoResultado">
            <dt>Identificador de execução</dt>
            <dd>
              <code>{ordem.identificadorExecucao}</code>
            </dd>
          </div>
        )}
        {aceita && ordem.identificadorOrdemServidor && (
          <div className="dadoResultado">
            <dt>Ordem no servidor</dt>
            <dd>
              <code>{ordem.identificadorOrdemServidor}</code>
            </dd>
          </div>
        )}
        {!aceita && ordem.motivoRejeicao && (
          <div className="dadoResultado motivoRejeicao">
            <dt>Motivo da rejeição</dt>
            <dd>{ordem.motivoRejeicao}</dd>
          </div>
        )}
      </dl>
    </section>
  );
};
