import { useLayoutEffect, useRef, useState, type FormEvent, type KeyboardEvent } from 'react';
import { CampoFormulario } from './CampoFormulario';
import { enviarOrdem } from '../services/ordemService';
import type { FalhaEnvio, ResultadoEnvio } from '../types/ordem';

interface PropriedadesFormulario {
  aoConcluir: (resultado: ResultadoEnvio) => void;
  aoIniciar: () => void;
}

interface ErrosFormulario {
  simbolo?: string;
  lado?: string;
  quantidade?: string;
  preco?: string;
}

const opcoesSimbolo = ['PETR4', 'VALE3', 'VIIA4'];

const formatadorReais = new Intl.NumberFormat('pt-BR', {
  maximumFractionDigits: 0,
});

const normalizarDigitos = (texto: string): string =>
  texto.replace(/\D/g, '').replace(/^0+(?=\d)/, '') || '0';

const formatarPreco = (centavosTexto: string): string => {
  const centavos = BigInt(centavosTexto);
  const reais = centavos / 100n;
  const centavosRestantes = (centavos % 100n).toString().padStart(2, '0');

  return `${formatadorReais.format(reais)},${centavosRestantes}`;
};

const validarPreco = (centavosTexto: string): { erro?: string } => {
  const centavos = BigInt(centavosTexto);

  if (centavos === 0n) {
    return { erro: 'O preço deve ser maior que zero.' };
  }

  if (centavos >= 100000n) {
    return { erro: 'O preço deve ser menor que R$ 1.000,00.' };
  }

  return {};
};

export const FormularioOrdem = ({ aoConcluir, aoIniciar }: PropriedadesFormulario) => {
  const [simbolo, definirSimbolo] = useState('PETR4');
  const [lado, definirLado] = useState<'Compra' | 'Venda'>('Compra');
  const [quantidade, definirQuantidade] = useState('');
  const [precoCentavos, definirPrecoCentavos] = useState('0');
  const [erros, definirErros] = useState<ErrosFormulario>({});
  const [enviando, definirEnviando] = useState(false);
  const [erroEnvio, definirErroEnvio] = useState<FalhaEnvio | null>(null);
  const envioEmAndamento = useRef(false);
  const campoPrecoRef = useRef<HTMLInputElement>(null);
  const posicionarCursorPreco = useRef(false);

  useLayoutEffect(() => {
    if (!posicionarCursorPreco.current) return;

    const campoPreco = campoPrecoRef.current;
    if (campoPreco && document.activeElement === campoPreco) {
      campoPreco.setSelectionRange(campoPreco.value.length, campoPreco.value.length);
    }

    posicionarCursorPreco.current = false;
  }, [precoCentavos]);

  const atualizarPreco = (digitos: string) => {
    posicionarCursorPreco.current = true;
    definirPrecoCentavos(normalizarDigitos(digitos));
  };

  const tratarTeclaPreco = (evento: KeyboardEvent<HTMLInputElement>) => {
    if (evento.key !== 'Backspace' && evento.key !== 'Delete') return;

    evento.preventDefault();
    const campoPreco = campoPrecoRef.current;
    const possuiSelecao = campoPreco && campoPreco.selectionStart !== campoPreco.selectionEnd;
    atualizarPreco(possuiSelecao ? '0' : precoCentavos.slice(0, -1));
  };

  const enviar = async (evento: FormEvent<HTMLFormElement>) => {
    evento.preventDefault();
    if (envioEmAndamento.current) return;

    const novosErros: ErrosFormulario = {};
    const quantidadeTemFormatoValido = /^\d+$/.test(quantidade);
    const quantidadeEmInteiro = quantidadeTemFormatoValido ? BigInt(quantidade) : 0n;
    const erroPreco = validarPreco(precoCentavos).erro;

    if (!opcoesSimbolo.includes(simbolo)) novosErros.simbolo = 'Selecione um símbolo válido.';
    if (!lado) novosErros.lado = 'Selecione compra ou venda.';
    if (quantidadeEmInteiro <= 0n || quantidadeEmInteiro >= 100000n) {
      novosErros.quantidade = 'Informe uma quantidade inteira entre 1 e 99.999.';
    }
    if (erroPreco) novosErros.preco = erroPreco;

    definirErros(novosErros);
    definirErroEnvio(null);

    if (Object.keys(novosErros).length > 0) return;

    const quantidadeInteira = Number(quantidadeEmInteiro);
    const precoNumerico = Number(BigInt(precoCentavos)) / 100;
    envioEmAndamento.current = true;
    definirEnviando(true);
    aoIniciar();
    const resultado = await enviarOrdem({
      simbolo,
      lado,
      quantidade: quantidadeInteira,
      preco: precoNumerico,
    });
    envioEmAndamento.current = false;
    definirEnviando(false);

    if (resultado.falha) {
      definirErroEnvio(resultado.falha);
      aoConcluir({ tipo: 'falha', falha: resultado.falha });
      return;
    }

    if (resultado.resposta) {
      aoConcluir({ tipo: 'resposta', ordem: resultado.resposta });
    }
  };

  return (
    <form className="formularioOrdem" onSubmit={enviar} noValidate>
      <div className="gradeCampos">
        <CampoFormulario id="simbolo" rotulo="Símbolo" erro={erros.simbolo}>
          <select
            className="campo-formulario__selecao"
            id="simbolo"
            value={simbolo}
            onChange={(evento) => definirSimbolo(evento.target.value)}
            aria-invalid={Boolean(erros.simbolo)}
            aria-describedby={erros.simbolo ? 'simbolo-erro' : undefined}
          >
            {opcoesSimbolo.map((opcao) => (
              <option key={opcao} value={opcao}>
                {opcao}
              </option>
            ))}
          </select>
        </CampoFormulario>

        <CampoFormulario id="lado" rotulo="Lado" erro={erros.lado}>
          <select
            className="campo-formulario__selecao"
            id="lado"
            value={lado}
            onChange={(evento) => definirLado(evento.target.value as 'Compra' | 'Venda')}
            aria-invalid={Boolean(erros.lado)}
            aria-describedby={erros.lado ? 'lado-erro' : undefined}
          >
            <option value="Compra">Compra</option>
            <option value="Venda">Venda</option>
          </select>
        </CampoFormulario>

        <CampoFormulario
          id="quantidade"
          rotulo="Quantidade"
          dica="Inteiro entre 1 e 99.999"
          erro={erros.quantidade}
        >
          <input
            className="campo-formulario__entrada"
            id="quantidade"
            type="text"
            inputMode="numeric"
            autoComplete="off"
            placeholder="Ex.: 100"
            value={quantidade}
            onChange={(evento) => definirQuantidade(evento.target.value.replace(/\D/g, ''))}
            aria-invalid={Boolean(erros.quantidade)}
            aria-describedby={erros.quantidade ? 'quantidade-erro' : 'quantidade-dica'}
          />
        </CampoFormulario>

        <CampoFormulario
          id="preco"
          rotulo="Preço por ativo"
          dica="Digite os números; os centavos são preenchidos automaticamente"
          erro={erros.preco}
        >
          <div className="campo-monetario">
            <span aria-hidden="true">R$</span>
            <input
              className="campo-formulario__entrada"
              id="preco"
              type="text"
              inputMode="numeric"
              autoComplete="off"
              value={formatarPreco(precoCentavos)}
              ref={campoPrecoRef}
              onChange={(evento) => atualizarPreco(evento.target.value)}
              onKeyDown={tratarTeclaPreco}
              onFocus={(evento) => {
                const comprimento = evento.currentTarget.value.length;
                evento.currentTarget.setSelectionRange(comprimento, comprimento);
              }}
              aria-invalid={Boolean(erros.preco)}
              aria-describedby={erros.preco ? 'preco-erro' : 'preco-dica'}
            />
          </div>
        </CampoFormulario>
      </div>

      <div className="rodapeFormulario">
        <p>Ordem enviada para processamento via FIX 4.4.</p>
        <button className="botao botao--primario" type="submit" disabled={enviando}>
          {enviando && <span className="indicadorCarregamento" aria-hidden="true" />}
          {enviando ? 'Enviando ordem…' : 'Enviar ordem'}
          {!enviando && <span aria-hidden="true">↗</span>}
        </button>
      </div>
      {erroEnvio && (
        <p className="avisoFormulario" role="status">
          {erroEnvio.titulo}
        </p>
      )}
      <span className="texto-acessivel" aria-live="polite">
        {enviando ? 'Enviando ordem para processamento.' : ''}
      </span>
    </form>
  );
};
