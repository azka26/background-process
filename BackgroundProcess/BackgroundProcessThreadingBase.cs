namespace BackgroundProcess;

public abstract class BackgroundProcessThreadingBase : IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource = null;
    private Timer? _timer = null;
    private Task? _task = null;
    private bool _isRunning = false;
    private bool _activityState = false;
    private bool _isOnCheckStopedActivity = false;

    public BackgroundProcessThreadingBase(int interval)
    {
        _timer = new Timer(ThreadActivity, "parameterValue", 0, interval);
    }

    protected void ErrorLog(Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
        if (e.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
        }
        Console.WriteLine("Stack Trace: " + e.StackTrace);
    }

    protected void ErrorLog(Exception e, string message)
    {
        Console.WriteLine($"Error: {e.Message}");
        if (e.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
        }
        Console.WriteLine("Stack Trace: " + e.StackTrace);
        Console.WriteLine($"Additional Info: {message}");
    }

    protected void ErrorLog(string message)
    {
        Console.WriteLine($"Error: {message}");
    }

    protected void InfoLog(string message)
    {
        Console.WriteLine($"Info: {message}");
    }

    private void CheckStopedActivity()
    {
        if (_isOnCheckStopedActivity)
        {
            return;
        }

        if (_cancellationTokenSource == null && _task == null)
        {
            return;
        }

        InfoLog("Activity is not running, checking for stopped activity...");
        _isOnCheckStopedActivity = true;
        try
        {
            if (_cancellationTokenSource == null)
            {
                return;
            }

            try
            {
                _cancellationTokenSource?.Cancel();
            }
            catch
            {
                // CancellationTokenSource already disposed, ignore
            }

            while (_isRunning)
            {
                InfoLog("Waiting for activity to stop...");
                Task.Delay(500).GetAwaiter().GetResult();
            }

            TryDispose(ref _cancellationTokenSource);
            TryDispose(ref _task);
        }
        catch
        {

        }
        finally
        {
            _isOnCheckStopedActivity = false;
        }
    }

    private void ThreadActivity(object? state)
    {
        if (!_activityState)
        {
            CheckStopedActivity();
            return;
        }

        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _task = Task.Run(async () =>
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            try
            {
                await Task.Delay(1000);
                var processId = Guid.NewGuid().ToString();
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        InfoLog("Cancellation requested, stopping activity.");
                        break;
                    }

                    await ThreadActivityAsync(processId, cancellationToken);
                    await Task.Delay(1000);
                }
            }
            catch (OperationCanceledException e)
            {
                InfoLog("Activity was cancelled.");
            }
            catch (Exception ex)
            {
                ErrorLog(ex, $"Exception in activity: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        });

        _task.GetAwaiter().GetResult();
        _isRunning = false;
    }

    protected abstract Task ThreadActivityAsync(string processId, CancellationToken cancellationToken);

    public void StartActivity()
    {
        _activityState = true;
        _isOnCheckStopedActivity = false;
        while (!_isRunning)
        {
            Thread.Sleep(100);
        }
        Console.WriteLine("Activity is already running.");
    }

    public void StopActivity()
    {
        _isOnCheckStopedActivity = false;
        _activityState = false;
        _cancellationTokenSource?.Cancel();
        while (_isRunning)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine("Activity stopped.");
    }

    private void TryDispose<T>(ref T? disposable) where T : class, IDisposable
    {
        try
        {
            InfoLog($"Disposing {typeof(T).Name}...");
            disposable?.Dispose();
        }
        catch (Exception ex)
        {
            InfoLog($"Failed to dispose {typeof(T).Name}: {ex.Message}");
        }
        finally
        {
            disposable = null;
            InfoLog($"{typeof(T).Name} disposed successfully.");
        }
    }

    public void Dispose()
    {
        StopActivity();
        
        InfoLog("Activity stopped, disposing resources...");
        TryDispose(ref _cancellationTokenSource);
        TryDispose(ref _task);
        TryDispose(ref _timer);
    }
}
