using System.Threading;

public abstract class RunableThread {

    private readonly Thread runnerThread;

    protected RunableThread()
    {
        runnerThread = new Thread(Run);
    }
    
    protected bool running { get; private set; }
    
    protected abstract void Run();

	public void Start () {
        running = true;
        runnerThread.Start();
	}
	
	public void Stop () {
        running = false;
        runnerThread.Join();
	}
}