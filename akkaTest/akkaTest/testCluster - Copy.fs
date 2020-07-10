#if INTERACTIVE
#r @"Z:\akka.net\src\core\Akka\bin\Debug\netstandard2.0\Akka.dll"
#r "nuget: Akka.Cluster"
#r "nuget: Akka.Cluster.Sharding"
#r "nuget: Akka.FSharp"
#r "nuget: Hyperion"
#r "nuget: Akka.Serialization.Hyperion"
#r @"C:\Users\anibal\.nuget\packages\akka.serialization.hyperion\1.4.7\lib\netstandard2.0\Akka.Serialization.Hyperion.dll"
#else
module akkaTest20200612
#endif

//(up)|(elcome)|(JOINING)|(xit)

open System
open System.IO
open Akka.Actor
open Akka.FSharp.Spawn
open Akka.Cluster
open Akka.FSharp
open Akka.Routing
open System.Collections.Generic
open Akka.Cluster.Routing
open Akka.Cluster.Sharding
open System.IO
open Akka.Configuration
open Akka.Cluster.Tools.Singleton
open Akka.Serialization
open Akka.Cluster
open Akka.Cluster

//ClusterSharding.Node.AutomaticJoin

let nodeName s = id s//sprintf "akkaNode-%s-%s-%s" s Environment.MachineName (Guid.NewGuid().ToString().Substring(0,5))
//System.Type.GetType("Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion")
//System.Type.GetType("System.Object")
let confCluster systemName port portSeed1 portSeed2 = 
    let conf = 
        Configuration.parse <| sprintf """
        akka {
            log-dead-letters  = off
            log-dead-letters-during-shutdown = off
                actor {
                  provider = cluster #"Akka.Cluster.ClusterActorRefProvider, Akka.Cluster"
                  serializers {
                    hyperion = "Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion"
                  }
                  serialization-bindings {
                    "System.Object" = hyperion
                  }
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
                    "akka.tcp://%s@10.28.199.142:%d", 
                    "akka.tcp://%s@10.28.199.142:%d"
                  ]
                  # when node cannot be reached within 10 sec, mark is as down
                  auto-down-unreachable-after = 300s
                  sharding {
                    least-shard-allocation-strategy.rebalance-threshold = 3
                  }
                }

                deployment {
                    /localactor {
                        router = consistent-hashing-pool
                        nr-of-instances = 5
                        virtual-nodes-factor = 10
                    }
                    /remoteactor1 {
                        router = consistent-hashing-pool
                        nr-of-instances = 5
                        #remote = "akka.tcp://%s@localhost:8080"
                    }
                    /remoteactor2 {
                        router = consistent-hashing-pool
                        nr-of-instances = 5
                        #remote = "akka.tcp://%s@localhost:8080"
                    }
                    /remoteactor3 {
                        router = consistent-hashing-pool
                        nr-of-instances = 5
                        #remote = "akka.tcp://%s@localhost:8080"
                    }
                }
            }
        }
        """ port systemName portSeed1 systemName portSeed2 systemName systemName systemName
    conf.WithFallback(ClusterSingletonManager.DefaultConfig())
let clusterSystem sysName p1 ps1 ps2 = System.create sysName <| confCluster sysName p1 ps1 ps2 
let cs02 = clusterSystem "test" 4053 4053 4053
//let cc = confCluster "test" 4054 4053 4053

let cluster = Cluster.Get(cs02)
cluster.Leave(cluster.SelfAddress)

let cs03 = clusterSystem "test" 4054 4053 4053


let shutdownTask = CoordinatedShutdown.Get(cs03).Run(CoordinatedShutdown.ClrExitReason.Instance)
shutdownTask.Wait()


//let props = Props.Create<EventStreamActor>()

let cs199142_104 = clusterSystem "test" 4104 4053 4053
let cluster = Cluster.Get(cs199142_104)
//let shutdownTask = CoordinatedShutdown.Get(cs04).Run(CoordinatedShutdown.ClrExitReason.Instance)
//shutdownTask.Wait()

let s12a1 = 
    spawn cs199142_104 "seed13actor1" <|
        //fun (mailbox: Actor<ClusterEvent.IClusterDomainEvent>) ->         
        fun (mailbox: Actor<obj>) ->         
            cluster.Subscribe (mailbox.Self, [| typeof<ClusterEvent.IClusterDomainEvent> |])
            //cluster.Subscribe (mailbox.Self, [| typeof<obj> |])
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
//s12a1.Path
//s12a1.Tell "orz"
//s12a1.GracefulStop(new TimeSpan(10L))
let cs15 = clusterSystem "test" 4150 4053 4053

let cluster = Cluster.Get(cs02)
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


let cs16 = clusterSystem "test" 4053 4053 4053

let confCs16 = confCluster "test" 4160 4053 4053

cs16.ActorSelection("/user").Tell("orz")

let reply = cs16.ActorOf<EventStreamActor>("reply")
    
let props1 = Props.Create(typeof<EventStreamActor>, [||])
    
let props2 = Props.Create(typeof<EventStreamActor>, [||])
    
let props3 = Props.Create(typeof<EventStreamActor>, [||])

let rConf = FromConfig.FromConfigSurrogate()
let sur = rConf.FromSurrogate(cs16)

let fc = FromConfig()
let csr = fc.CreateRouter cs16


//let remote1 = cs16.ActorOf(props1.WithRouter(FromConfig.Instance), "remoteactor1")

//let remote2 = cs16.ActorOf(props2.WithRouter(FromConfig.Instance), "remoteactor2")

//let remote3 = cs16.ActorOf(props3.WithRouter(FromConfig.Instance), "remoteactor3")


let props = Props.Create<EventStreamActor>().WithRouter(new RoundRobinPool(5))
let actor = cs16.ActorOf(props, "worker")

/////////////////////////////////
type EchoServer =
    inherit Actor

    override x.OnReceive message =
        match message with
        | :? string as msg -> printfn "Hello %s %d" msg Threading.Thread.CurrentThread.ManagedThreadId
        | _ -> 
            printfn "um---------------->"
            failwith "unknown message"

let echoServer = cs16.ActorOf(Props(typedefof<EchoServer>, Array.empty))

echoServer <! "F#!"
////////////////////////////

let handler (mailbox: Actor<obj>) msg =
    match box msg with
    | :? FileInfo as fi ->
        let reader = new StreamReader(fi.OpenRead())
        reader.ReadToEndAsync() |> Async.AwaitTask |!> mailbox.Self
    | :? string as content ->
        printfn "File content: %s" content
    | _ -> mailbox.Unhandled()

let aref = spawn cs16 "my-actor" (actorOf2 handler)
aref <! new FileInfo "Akka.xml"

////////////////////////////

let props = Props.Create<EchoServer>().WithRouter(new RoundRobinPool(5))
let actor = cs16.ActorOf(props, "workers")


let hashGroup = cs16.ActorOf(Props.Empty.WithRouter(ConsistentHashingGroup(confCs16)))
//Task.Delay(500).Wait()

let routee1 = Routee.FromActorRef(actor)
hashGroup.Tell(new AddRoutee(routee1))



let sd = SortedDictionary()
sd.Add(123,456)
let ch = ConsistentHash(sd, 1)

let message = ConsistentHashableEnvelope(123, 1)
hashGroup.Tell(message, reply)

let cs01 = clusterSystem "test" 4001 4001 4002
let cs02 = clusterSystem "test" 4002 4001 4002

let clusterPoolSetting = new ClusterRouterPoolSettings(2, 5, true)


let cpProps = Props.Create<EchoServer>().WithRouter(new ClusterRouterPool(new RoundRobinPool(5), clusterPoolSetting))
let actor = cs02.ActorOf(cpProps, "worker1")

actor <! "orz"
actor <! 123


cs02.ActorSelection("/user/worker1/").Tell("ffff")

cs02.ActorSelection("/user/worker0/").ResolveOne(new TimeSpan(0, 0, 0, 0, 200)).Result <! "orz"

cs01.ActorSelection("akka.tcp://test@10.28.199.142:4002/user/worker1/").ResolveOne(new TimeSpan(0, 0, 0, 0, 200)).Result <! "orz"
cs02.ActorSelection("/user/worker0/").ResolveOne(new TimeSpan(0, 0, 0, 0, 200)).Result <! "orz"

 
let cluster = Cluster.Get(cs02)
(cluster.State.Members.Item 0).Address


//use ClusterSharding = Akka.Cluster.Sharding.ClusterSharding

let conf : Config = Configuration.parse @"
    akka {
      actor {
        provider = cluster
        serializers {
          hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
        }
        serialization-bindings {
          ""System.Object"" = hyperion
        }
      }
      remote {
        dot-netty.tcp {
          public-hostname = ""localhost""
          hostname = ""localhost""
          port = 0
        }
      }
      cluster {
        auto-down-unreachable-after = 5s
        sharding {
          least-shard-allocation-strategy.rebalance-threshold = 3
        }
      }

    }"

let system = 
    //ActorSystem.Create("sharded-cluster-system", conf.WithFallback(ClusterSingletonManager.DefaultConfig()))
    clusterSystem "test" 4003 4001 4002

let cluster = Cluster.Get(system)

let sharding = ClusterSharding.Get(system)


type ShardEnvelope (entityId:string, payload:obj) =
    member val EntityId = entityId with get, set
    member val Payload = payload with get, set



type MessageExtractor (i:int) =
    inherit HashCodeMessageExtractor(i)

    override this.EntityId(message:obj) = 
        (message :?> ShardEnvelope).EntityId

    override this.EntityMessage(message:obj) = 
        (message :?> ShardEnvelope).Payload


let shardRegion = 
    sharding.Start(
        typeName = "customer",
        entityProps = Props.Create<EchoServer>(),
        settings = ClusterShardingSettings.Create(system),
        messageExtractor = new MessageExtractor(10))

