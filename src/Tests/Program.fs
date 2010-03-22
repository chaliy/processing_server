do FsSpec.Runner.Run()

open Shared

//open System.Threading
//open System.Threading.Tasks
//
//let started = new Event<string>()
//let completed = new Event<string>()
//
//let fakeTask task =
//    started.Trigger(task)        
//    for i in [1..500000] do
//        System.Text.RegularExpressions.Regex.IsMatch(System.Guid.NewGuid().ToString(), i.ToString()) |> ignore
//    completed.Trigger(task)
//
//printfn "Task generation started"
//let tasks = 
//    [for i in [0..1000] do    
//        yield new Task(new System.Action(fun () -> fakeTask(i.ToString())), TaskCreationOptions.PreferFairness) ]
//
//started.Publish |> Event.add(fun task ->    
//    let av = tasks              
//             |> Seq.filter(fun t -> t.Status = TaskStatus.Running)
//             |> Seq.length 
//    printfn "Task started : %s (queue length : %i)" task av )
//
//completed.Publish |> Event.add(fun task ->     
//    let av = tasks              
//             |> Seq.filter(fun t -> t.Status = TaskStatus.Running)
//             |> Seq.length
//    printfn "Task completed : %s (queue length : %i)" task av )
//
//tasks |> Seq.iter(fun t -> t.Start())
//                    
//printfn "Task generation completed"
//
//System.Console.ReadLine() |> ignore

//let tr = new Trottler(System.TimeSpan.FromSeconds(10.0))
//tr.Pushed.Add(fun _ -> printfn "Pushed %s" (System.DateTime.UtcNow.ToLongTimeString()))
//
//printfn "Pushing after 0 %s" (System.DateTime.UtcNow.ToLongTimeString())
//tr.Ping()
//printfn "Pushing after 0 %s" (System.DateTime.UtcNow.ToLongTimeString())
//tr.Ping()
//System.Threading.Thread.Sleep(26000)
//printfn "Pushing after 26 %s" (System.DateTime.UtcNow.ToLongTimeString())
//tr.Ping()
//System.Threading.Thread.Sleep(5000)
//printfn "Pushing after 5 %s" (System.DateTime.UtcNow.ToLongTimeString())
//tr.Ping()
//System.Threading.Thread.Sleep(5000)
//printfn "Pushing after 5 %s" (System.DateTime.UtcNow.ToLongTimeString())
//tr.Ping()
//
//System.Console.ReadLine() |> ignore