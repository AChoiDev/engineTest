using EntityID = int;
using Vector3 = System.Numerics.Vector3;
using Color = Raylib_cs.Color;
using MyEngine;

using AsyncMonitor = Nito.AsyncEx.AsyncMonitor;


public static class World {
    private static Dictionary<EntityID, object> entityMap = [];
    // private static int nextEntityID = 0;

    private static int expectedExecutionDependents = 0;
    private static int executionsFlagged = 0;

    private static int tickCounter = 0;

    private static AsyncMonitor monitor = new();



    public static async Task Run() {
        await Task.Delay(1000);
        int entityIDCounter = 0;

        // executions to add 
        var executionStartQueue = new HashSet<EntityID>();
        // the executions that are yielding, paused, or running currently
        var executionQueue = new Dictionary<EntityID, Task>();
        // setup
        const int MaxColumns = 20;

        for (int i = 0; i < MaxColumns; i++)
        {
            var thingy = new Random();
            var height = thingy.Next(1, 12);
            var box = new ProtoRLBox{
                Height = height,
                Position = new Vector3(thingy.Next(-15, 15), height / 2, thingy.Next(-15, 15)),
                Color = new Color(thingy.Next(20, 255), thingy.Next(10, 55), 30, 255)
            };
            // add entity
            entityMap[entityIDCounter] = box;
            if (box is IExecutable) {
                executionStartQueue.Add(entityIDCounter);
            }
            entityIDCounter += 1;
        }

        while (true) {

            await Task.Delay(10);
            using (await monitor.EnterAsync()) {
                // clear variables for executions this tick
                executionsFlagged = 0;
                expectedExecutionDependents = executionQueue.Count + executionStartQueue.Count;
                tickCounter += 1;
                monitor.PulseAll(); // wake up all yielding executions
            }

            // start new executions
            foreach (var entID in executionStartQueue) {
                var exe = (IExecutable)entityMap[entID];
                executionStartQueue.Remove(entID);
                executionQueue.Add(entID, executionWrapper(exe));
            }

            // wait for executions to be done
            using (await monitor.EnterAsync()) {
                while (expectedExecutionDependents != executionsFlagged) {
                    await monitor.WaitAsync();
                }
            }

            // no executions should be running now

            // remove all executions that have completed
            foreach (var (id, task) in executionQueue) {
                if (task.IsCompleted) {
                    executionQueue.Remove(id);
                }
            }
        }
    }

    private static async Task executionWrapper(IExecutable exe)
    {
        await exe.Execute();
        using (await monitor.EnterAsync())
        {
            executionsFlagged += 1;

            if (executionsFlagged == expectedExecutionDependents)
            {
                // world admin can continue
                monitor.PulseAll();
            }
            // TODO: flag this execution as done
            return;
        }
        // todo: wait indefinitely here
        // await Task.Delay(10000);
    }

    public static void Render() {
        foreach (var ent in entityMap.Values) {
            (ent as IRenderable)?.Render();
        }
    }
    public static async Task Yield() {
        await Task.CompletedTask;

        var preTick = tickCounter;
        using (await monitor.EnterAsync()) {
            executionsFlagged += 1;
            if (executionsFlagged == expectedExecutionDependents) {
                // world admin can continue
                monitor.PulseAll();
            }
            while (tickCounter != preTick + 1) {
                // wait until admin has set the tick counter to the next tick
                await monitor.WaitAsync();
                if (tickCounter > preTick + 1) {
                    throw new System.Exception("Tick counter went up more than one");
                }
            }
        }
    }
}