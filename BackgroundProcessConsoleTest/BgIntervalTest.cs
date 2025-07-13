using System;
using BackgroundProcess;

namespace BackgroundProcessConsoleTest;

public class BgIntervalTest : BackgroundProcessThreadingBase
{
    private string _instanceName;
    public BgIntervalTest(string instanceName) : base(1000)
    {
        _instanceName = instanceName;
    }

    protected override async Task ThreadActivityAsync(string processId, CancellationToken cancellationToken)
    {
        InfoLog($"Thread {_instanceName} - {processId} started at {DateTime.Now}");
        for (int i = 0; i < 5; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                InfoLog($"Thread {_instanceName} - {processId} cancelled at {DateTime.Now}");
                return;
            }

            InfoLog($"Thread {_instanceName} - {processId} working... {i + 1}");
            await Task.Delay(500);
        }

        InfoLog($"Thread {_instanceName} - {processId} completed at {DateTime.Now}");
    }
}
