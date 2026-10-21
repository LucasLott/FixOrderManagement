import type { ReactNode } from 'react';

interface PropriedadesCampo {
  id: string;
  rotulo: string;
  dica?: string;
  erro?: string;
  children: ReactNode;
}

export const CampoFormulario = ({ id, rotulo, dica, erro, children }: PropriedadesCampo) => {
  return (
    <div className="campo-formulario">
      <label className="campo-formulario__rotulo" htmlFor={id}>
        {rotulo}
      </label>
      {children}
      {erro && (
        <span className="campo-formulario__erro" id={`${id}-erro`}>
          {erro}
        </span>
      )}
      {!erro && dica && (
        <span className="campo-formulario__dica" id={`${id}-dica`}>
          {dica}
        </span>
      )}
    </div>
  );
};
