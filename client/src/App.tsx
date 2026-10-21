import { useState } from 'react';
import { FormularioOrdem } from './components/FormularioOrdem';
import { ResultadoOrdem } from './components/ResultadoOrdem';
import type { ResultadoEnvio } from './types/ordem';

function App() {
  const [resultado, definirResultado] = useState<ResultadoEnvio | null>(null);
  const [enviando, definirEnviando] = useState(false);

  return (
    <main className="pagina">
      <header className="cabecalho">
        <a className="marca" href="#inicio" aria-label="FixOrderManagement, início">
          <span className="marcaIcone" aria-hidden="true">
            <span />
            <span />
            <span />
          </span>
          <span>
            Fix<span className="marcaDestaque">Order</span>Management
          </span>
        </a>
        <div className="seloAmbiente">
          <span /> FIX 4.4 · ORDEM DIRETA
        </div>
      </header>

      <section className="introducao" id="inicio">
        <div>
          <p className="sobretitulo">OPERAÇÕES · RENDA VARIÁVEL</p>
          <h1>
            Envie sua ordem
            <br />
            <span>com clareza.</span>
          </h1>
          <p className="textoIntroducao">
            Sistema de envio e processamento de ordens financeiras, com comunicação via protocolo
            FIX 4.4.
          </p>
        </div>
        <div className="indicadorProtocolo" aria-label="Protocolo FIX versão 4.4">
          <span className="indicadorLinha" />
          <span>
            <strong>FIX</strong>
            <small>PROTOCOLO 4.4</small>
          </span>
        </div>
      </section>

      <section className="painelPrincipal" aria-label="Envio e resultado da ordem">
        <div className="cartaoFormulario">
          <div className="cabecalhoCartao">
            <div className="numeroEtapa">01</div>
            <div>
              <p className="sobretitulo">NOVA SOLICITAÇÃO</p>
              <h2>Dados da ordem</h2>
            </div>
          </div>
          <FormularioOrdem
            aoIniciar={() => definirEnviando(true)}
            aoConcluir={(novoResultado) => {
              definirResultado(novoResultado);
              definirEnviando(false);
            }}
          />
        </div>

        <div className="colunaResultado" aria-busy={enviando}>
          <div className="tituloResultado">
            <p className="sobretitulo">ACOMPANHAMENTO</p>
            <span className={enviando ? 'estadoApi estadoEnviando' : 'estadoApi'}>
              <span />
              {enviando ? 'Processando' : 'API'}
            </span>
          </div>
          <ResultadoOrdem resultado={resultado} />
        </div>
      </section>

      <footer className="rodapePagina">
        <span>FIXORDERMANAGEMENT</span>
        <span>Ordens enviadas são processadas pelo serviço Accumulator.</span>
        <span>AMBIENTE DE DEMONSTRAÇÃO</span>
      </footer>
    </main>
  );
}

export default App;
