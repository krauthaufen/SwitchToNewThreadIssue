module ThreadTest

open System
open System.Threading
open System.Threading.Tasks

let printLock = obj()

// start a new thread (using Async.SwitchToNewThread)
let startThread (id : int)  = 
    async {
        do! Async.SwitchToNewThread()
        let tid = System.Threading.Thread.CurrentThread.ManagedThreadId
              
        // avoid stdout races
        lock printLock (fun () ->
            printfn "%d is threadpool: %A" tid System.Threading.Thread.CurrentThread.IsThreadPoolThread
        )
        while true do
            let c = System.Threading.Thread.CurrentThread.ManagedThreadId
            if c <> tid then
                printfn "ERROR"
            System.Threading.Thread.Sleep(1000)

    } |> Async.Start
                            
    
// start a task measuring its response time and printing it if > 100ms  
let startTask ()  = 
    let sw = System.Diagnostics.Stopwatch.StartNew()
    async {
        do! Async.SwitchToThreadPool()
        sw.Stop()
        if sw.Elapsed.TotalSeconds > 0.1 then
            
            printfn "long: %A" sw.Elapsed.TotalSeconds
        return sw.Elapsed.TotalSeconds
    } |> Async.StartAsTask

let run () =
    let cnt = Environment.ProcessorCount
    for i in 0 .. cnt - 1 do
        startThread i

    while true do
        startTask().Wait()
