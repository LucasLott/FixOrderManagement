using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Worker.Infrastructure.Fix
{
    public class FabricaExecutionReport
    {
        public ExecutionReport Criar(NewOrderSingle ordem,
                                     bool aceita,
                                     string? motivo)
        {
            var relatorio = new ExecutionReport(new OrderID(ordem.ClOrdID.Value),
                                                new ExecID(Guid.NewGuid().ToString("N")),
                                                new ExecType(aceita ? ExecType.NEW : ExecType.REJECTED),
                                                new OrdStatus(aceita ? OrdStatus.NEW : OrdStatus.REJECTED),
                                                new Symbol(ordem.Symbol.Value),
                                                new Side(ordem.Side.Value),
                                                new LeavesQty(aceita ? ordem.OrderQty.Value : 0m),
                                                new CumQty(0m),
                                                new AvgPx(0m));

            relatorio.SetField(new ClOrdID(ordem.ClOrdID.Value));
            relatorio.SetField(new OrderQty(ordem.OrderQty.Value));
            relatorio.SetField(new Price(ordem.Price.Value));

            if (!aceita && motivo != null)
            {
                relatorio.SetField(new Text(motivo));
            }

            return relatorio;
        }
    }
}
