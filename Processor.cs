using System;
using System.Threading;
using System.Threading.Tasks;

public static class Processor
{


    // public Task RunAsync(CancellationToken stoppingToken)
    // {
    //     ScheduleTask(async () => biPrice = await Binance.GetBidPriceAsync("BTCEUR"), 1, stoppingToken);



    // }


    static void ScheduleTask(Action action, int seconds, CancellationToken token)
    {
        if (action == null)
            return;
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                action();
                await Task.Delay(TimeSpan.FromSeconds(seconds), token);
            }
        }, token);
    }

}