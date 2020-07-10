#if INTERACTIVE
#r @"C:\Users\anibal\.nuget\packages\system.reflection.typeextensions\4.7.0\lib\net46\System.Reflection.TypeExtensions.dll"
#r @"Z:\akka.net\src\core\Akka\bin\Debug\netstandard2.0\Akka.dll"
#r @"Z:\akka.net\src\core\Akka.Remote\bin\Debug\netstandard2.0\Akka.Remote.dll"
#r @"C:\Users\anibal\.nuget\packages\akka.serialization.hyperion\1.4.7\lib\netstandard2.0\Akka.Serialization.Hyperion.dll"
#r "nuget: Akka.Cluster"
#r "nuget: Akka.Cluster.Sharding"
#r "nuget: Akka.FSharp"
#r "nuget: Hyperion"
#r "nuget: Akka.Serialization.Hyperion"

#else
module akkaTest2
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
open Akka.Persistence

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


type EchoServer =
    inherit Actor

    override x.OnReceive message =
        match message with
        | :? string as msg -> printfn "Hello %s %d" msg Threading.Thread.CurrentThread.ManagedThreadId
        | _ -> 
            printfn "um---------------->"
            failwith "unknown message"




let cs01 = clusterSystem "test" 4001 4001 4002
let cs02 = clusterSystem "test" 4002 4001 4002

//let clusterPoolSetting = new ClusterRouterPoolSettings(2, 5, true)


//let cpProps = Props.Create<EchoServer>().WithRouter(new ClusterRouterPool(new RoundRobinPool(5), clusterPoolSetting))
//let actor = cs02.ActorOf(cpProps, "worker1")

//cs02.ActorSelection("/user/worker1/").Tell("ffff")

//cs02.ActorSelection("/user/worker0/").ResolveOne(new TimeSpan(0, 0, 0, 0, 200)).Result <! "orz"

//cs01.ActorSelection("akka.tcp://test@10.28.199.142:4002/user/worker1/").ResolveOne(new TimeSpan(0, 0, 0, 0, 200)).Result <! "orz"
//cs02.ActorSelection("/user/worker0/").ResolveOne(new TimeSpan(0, 0, 0, 0, 200)).Result <! "orz"

 
//let cluster = Cluster.Get(cs02)
//(cluster.State.Members.Item 0).Address


//use ClusterSharding = Akka.Cluster.Sharding.ClusterSharding

//let conf : Config = Configuration.parse @"
//    akka {
//      actor {
//        provider = cluster
//        serializers {
//          hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
//        }
//        serialization-bindings {
//          ""System.Object"" = hyperion
//        }
//      }
//      remote {
//        dot-netty.tcp {
//          public-hostname = ""localhost""
//          hostname = ""localhost""
//          port = 0
//        }
//      }
//      cluster {
//        auto-down-unreachable-after = 5s
//        sharding {
//          least-shard-allocation-strategy.rebalance-threshold = 3
//        }
//      }
//    }"

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

type PurchaseItem () =
    member val ItemName = Unchecked.defaultof<string> with get, set
    member this.PurchaseItem(itemName:string) =
        this.ItemName <- itemName

type ItemPurchased () =
    member val ItemName = Unchecked.defaultof<string> with get, set
    member this.ItemPurchased(itemName:string) =
        this.ItemName <- itemName                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
    
type Customer () as this = 
    inherit ReceivePersistentActor ()
    let mutable purchasedItems = new List<string>() :> ICollection<string>
    do  
        this.SetReceiveTimeout(new Nullable<TimeSpan>(TimeSpan.FromSeconds(60.0)))
        this.Recover<ItemPurchased>(fun (purchased:ItemPurchased) -> purchasedItems.Add(purchased.ItemName))
        let act0 = 
            new Action<ItemPurchased>(
                fun purchased ->
                    purchasedItems.Add(purchased.ItemName)
                    let name = Uri.UnescapeDataString(this.self.Path.Name)
                    printfn "%s purchased '%s'.\nAll items: [%s]\n--------------------------" name purchased.ItemName (String.Join(", ", purchasedItems))
                )
        let act = 
            new Action<PurchaseItem>(
                fun (purchase:PurchaseItem) ->
                    let ip = new ItemPurchased()
                    ip.ItemPurchased purchase.ItemName
                    this.Persist(ip, act0)
                )
        this.Command<PurchaseItem>(act)
        
    override val PersistenceId = ReceivePersistentActor.Context.Parent.Path.Name + "/" + ReceivePersistentActor.Context.Self.Path.Name with get
    member this.self : IActorRef = this.Self
    member this._purchasedItems 
        with get () = purchasedItems
        and set (value) = purchasedItems <- value




let shardRegion = 
    sharding.Start(
        typeName = "Customer",
        entityProps = Props.Create<Customer>(),
        settings = ClusterShardingSettings.Create(system),
        messageExtractor = new MessageExtractor(10))

