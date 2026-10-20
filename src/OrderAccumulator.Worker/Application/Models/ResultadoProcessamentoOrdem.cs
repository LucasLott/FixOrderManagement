using OrderAccumulator.Worker.Domain.Enums;

namespace OrderAccumulator.Worker.Application.Models
{
    public class ResultadoProcessamentoOrdem
    {
        public string IdentificadorOrdem { get; }
        public SimboloAtivo Simbolo { get; }
        public bool Aceita { get; }
        public decimal ExposicaoAtual { get; }
        public decimal ExposicaoProjetada { get; }
        public string? MotivoRejeicao { get; }

        public ResultadoProcessamentoOrdem(string _identificadorOrdem,
                                           SimboloAtivo _simbolo,
                                           bool _aceita,
                                           decimal _exposicaoAtual,
                                           decimal _exposicaoProjetada,
                                           string? _motivoRejeicao = null)
        {
            IdentificadorOrdem = _identificadorOrdem;
            Simbolo = _simbolo;
            Aceita = _aceita;
            ExposicaoAtual = _exposicaoAtual;
            ExposicaoProjetada = _exposicaoProjetada;
            MotivoRejeicao = _motivoRejeicao;
        }
    }
}
