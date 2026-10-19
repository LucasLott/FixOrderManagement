using OrderAccumulator.Worker.Domain.Enums;

namespace OrderAccumulator.Worker.Domain.Entities
{
    public class Ordem
    {
        public string Identificador { get; }
        public SimboloAtivo Simbolo { get; }
        public LadoOrdem Lado { get; }
        public int Quantidade { get; }
        public decimal Preco { get; }

        public Ordem(string _identificador,
                     SimboloAtivo _simbolo,
                     LadoOrdem _lado,
                     int _quantidade,
                     decimal _preco)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(_identificador);

            if (!Enum.IsDefined(_simbolo))
            {
                throw new ArgumentOutOfRangeException(nameof(_simbolo), _simbolo, "Símbolo inválido.");
            }

            if (!Enum.IsDefined(_lado))
            {
                throw new ArgumentOutOfRangeException(nameof(_lado), _lado, "Lado inválido.");
            }

            if (_quantidade < 1 || _quantidade > 99_999)
            {
                throw new ArgumentOutOfRangeException(nameof(_quantidade), _quantidade, "A quantidade deve estar entre 1 e 99.999.");
            }

            if (_preco <= 0m || _preco >= 1_000m || _preco % 0.01m != 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(_preco), _preco, "O preço deve ser maior que zero, menor que 1.000 e múltiplo de 0,01.");
            }

            Identificador = _identificador;
            Simbolo = _simbolo;
            Lado = _lado;
            Quantidade = _quantidade;
            Preco = _preco;
        }
    }
}
