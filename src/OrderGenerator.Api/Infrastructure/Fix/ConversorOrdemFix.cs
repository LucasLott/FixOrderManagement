using OrderGenerator.Api.Application.Models;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderGenerator.Api.Infrastructure.Fix
{
    public sealed class ConversorOrdemFix
    {
        public NewOrderSingle Converter(SolicitacaoOrdem solicitacao)
        {
            var ordem = new NewOrderSingle(new ClOrdID(solicitacao.Identificador),
                                           new Symbol(solicitacao.Simbolo.ToString()),
                                           new Side(solicitacao.Lado == LadoSolicitacaoOrdem.Compra ? Side.BUY : Side.SELL),
                                           new TransactTime(DateTime.UtcNow),
                                           new OrdType(OrdType.LIMIT));

            ordem.Set(new Symbol(solicitacao.Simbolo.ToString()));
            ordem.Set(new OrderQty(solicitacao.Quantidade));
            ordem.Set(new Price(solicitacao.Preco));
            
            return ordem;
        }
    }
}
