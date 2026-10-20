using OrderAccumulator.Worker.Domain.Entities;
using OrderAccumulator.Worker.Domain.Enums;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Worker.Infrastructure.Fix
{
    public class ConversorOrdemFix
    {
        public Ordem Converter(NewOrderSingle mensagem)
        {
            string identificador = mensagem.ClOrdID.Value;
            string simboloRecebido = mensagem.Symbol.Value;
            char ladoRecebido = mensagem.Side.Value;
            decimal quantidadeRecebida = mensagem.OrderQty.Value;
            decimal preco = mensagem.Price.Value;
            char tipoOrdem = mensagem.OrdType.Value;

            if (tipoOrdem != OrdType.LIMIT)
            {
                throw new ArgumentException("Somente ordens limitadas são aceitas.");
            }

            SimboloAtivo simbolo = simboloRecebido switch
            {
                "PETR4" => SimboloAtivo.PETR4,
                "VALE3" => SimboloAtivo.VALE3,
                "VIIA4" => SimboloAtivo.VIIA4,
                _ => throw new ArgumentException("Símbolo não permitido.")
            };

            LadoOrdem lado = ladoRecebido switch
            {
                Side.BUY => LadoOrdem.Compra,
                Side.SELL => LadoOrdem.Venda,
                _ => throw new ArgumentException("Lado da ordem não permitido.")
            };

            if (quantidadeRecebida != decimal.Truncate(quantidadeRecebida)
                || quantidadeRecebida < 1m || quantidadeRecebida > int.MaxValue)
            {
                throw new ArgumentException("A quantidade deve ser um número inteiro positivo válido.");
            }

            return new Ordem(identificador,
                             simbolo,
                             lado,
                             (int)quantidadeRecebida,
                             preco);
        }
    }
}
