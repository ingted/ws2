#if INTERACTIVE
#r "nuget: Akka.Cluster"
#r "nuget: Akka.FSharp"
#endif

open System
open System.IO
open Akka.Actor
open Akka.FSharp.Spawn
open Akka.Cluster
open Akka.FSharp

let nodeName s = id s//sprintf "akkaNode-%s-%s-%s" s Environment.MachineName (Guid.NewGuid().ToString().Substring(0,5))

let confCluster systemName port portSeed1 portSeed2 = 
    Configuration.parse <| sprintf """
    akka {
        log-dead-letters  = off
        log-dead-letters-during-shutdown = off
        actor {
              provider = "Akka.Cluster.ClusterActorRefProvider, Akka.Cluster"
            }
            remote {
              log-remote-lifecycle-events = off
              helios.tcp {
                hostname = "10.28.199.142"
                port = %d        
              }
            }
            cluster {
              roles = ["seed"]  # custom node roles
              seed-nodes = [
                "akka.tcp://%s@10.28.112.112:%d", 
                "akka.tcp://%s@10.28.199.142:%d"
              ]
              # when node cannot be reached within 10 sec, mark is as down
              auto-down-unreachable-after = 300s
            }
        }
    }
    """ port systemName portSeed1 systemName portSeed2

let clusterSystem sysName p1 ps1 ps2 = System.create sysName <| confCluster sysName p1 ps1 ps2
let cs02 = clusterSystem "test" 4054 4053 4053

let cluster = Cluster.Get(cs02)
cluster.Leave(cluster.SelfAddress)

let cs03 = clusterSystem "test" 4054 4053 4053


let shutdownTask = CoordinatedShutdown.Get(cs03).Run(CoordinatedShutdown.ClrExitReason.Instance)
shutdownTask.Wait()


//let props = Props.Create<EventStreamActor>()

let cs04 = clusterSystem "test" 4054 4053 4053

//let shutdownTask = CoordinatedShutdown.Get(cs04).Run(CoordinatedShutdown.ClrExitReason.Instance)
//shutdownTask.Wait()

let s12a1 = 
    spawn cs04 "seed13actor1" <|
        fun (mailbox: Actor<ClusterEvent.IClusterDomainEvent>) ->         
            cluster.Subscribe (mailbox.Self, [| typeof<ClusterEvent.IClusterDomainEvent> |])
            mailbox.Defer <| fun () -> cluster.Unsubscribe mailbox.Self
            printfn "Created an actor on node [%A] with roles [%s]" cluster.SelfAddress (String.Join(",", cluster.SelfRoles))
            let rec seed () = 
                actor {
                    let! message = mailbox.Receive() 
                    match message with
                    | :? ClusterEvent.MemberJoined as event -> printfn ">>> Member %s Joined the Cluster at %O" event.Member.Address.Host DateTime.Now
                    | :? ClusterEvent.MemberLeft as event -> printfn ">>> Member %s Left the Cluster at %O" event.Member.Address.Host DateTime.Now
                    | other -> printfn ">>> Cluster Received event %O at %O" other DateTime.Now
                    return! seed()
                }
            seed()

let cs15 = clusterSystem "test" 4150 4053 4053


let seed13actor2 =
    spawn cs15 "seed13actor2" <|
    fun (mailbox: Actor<obj>) -> 
        let cluster = Cluster.Get cs15
        cluster.Subscribe (mailbox.Self, [| typeof<ClusterEvent.IMemberEvent> |])
        mailbox.Defer <| fun () -> cluster.Unsubscribe mailbox.Self
        printfn "==[%s]== Created an actor on node [%A] with roles [%s]" "seed13actor2" cluster.SelfAddress (String.Join(",", cluster.SelfRoles))
        let rec seed () = 
            actor {
                let! message = mailbox.Receive() 
                match message with
                | :? ClusterEvent.IMemberEvent as event -> printfn "==[%s]== Cluster member event %s at %O" "seed13actor2" event.Member.Address.Host DateTime.Now
                | other -> printfn "==[%s]== Cluster Received event %O at %O" "seed13actor2" other DateTime.Now
                return! seed()
            }
        seed()


let reply = cs15.ActorOf<ReplyActor>("reply")
    
let props1 = Props.Create(typeof<SomeActor>, [||])
    
let props2 = Props.Create(typeof<SomeActor>, [||])
    
let props3 = Props.Create(typeof<SomeActor>, [||])