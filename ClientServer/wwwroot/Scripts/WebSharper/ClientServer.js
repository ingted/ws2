(function()
{
 "use strict";
 var Global,testFrom0,Client,SC$1,GeneratedPrintf,ClientServer_JsonEncoder,ClientServer_JsonDecoder,WebSharper,UI,Next,Var,Html,Client$1,Tags,Concurrency,Owin,WebSocket,Client$2,WithEncoding,JSON,IntelliFactory,Runtime,Utils,$,Strings,Arrays,Submitter,View,Remoting,AjaxRemotingProvider,MatchFailureException,Doc,AttrProxy,ClientSideJson,Provider;
 Global=self;
 testFrom0=Global.testFrom0=Global.testFrom0||{};
 Client=testFrom0.Client=testFrom0.Client||{};
 SC$1=Global.StartupCode$ClientServer$Client=Global.StartupCode$ClientServer$Client||{};
 GeneratedPrintf=Global.GeneratedPrintf=Global.GeneratedPrintf||{};
 ClientServer_JsonEncoder=Global.ClientServer_JsonEncoder=Global.ClientServer_JsonEncoder||{};
 ClientServer_JsonDecoder=Global.ClientServer_JsonDecoder=Global.ClientServer_JsonDecoder||{};
 WebSharper=Global.WebSharper;
 UI=WebSharper&&WebSharper.UI;
 Next=UI&&UI.Next;
 Var=Next&&Next.Var;
 Html=WebSharper&&WebSharper.Html;
 Client$1=Html&&Html.Client;
 Tags=Client$1&&Client$1.Tags;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Owin=WebSharper&&WebSharper.Owin;
 WebSocket=Owin&&Owin.WebSocket;
 Client$2=WebSocket&&WebSocket.Client;
 WithEncoding=Client$2&&Client$2.WithEncoding;
 JSON=Global.JSON;
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 Utils=WebSharper&&WebSharper.Utils;
 $=Global.jQuery;
 Strings=WebSharper&&WebSharper.Strings;
 Arrays=WebSharper&&WebSharper.Arrays;
 Submitter=Next&&Next.Submitter;
 View=Next&&Next.View;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 MatchFailureException=WebSharper&&WebSharper.MatchFailureException;
 Doc=Next&&Next.Doc;
 AttrProxy=Next&&Next.AttrProxy;
 ClientSideJson=WebSharper&&WebSharper.ClientSideJson;
 Provider=ClientSideJson&&ClientSideJson.Provider;
 Client.Send=function(serverReceive)
 {
  var container,b;
  function writen(fmt)
  {
   return fmt(function(s)
   {
    Var.Set(Client.filterResult(),Client.filterResult().c.concat([s+"\n"]));
    container.Dom.appendChild(self.document.createTextNode(s+"\n"));
   });
  }
  container=Tags.Tags().NewTag("pre",[]);
  Concurrency.Start((b=null,Concurrency.Delay(function()
  {
   return Concurrency.Bind(WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((ClientServer_JsonEncoder.j())(a));
   },function(a)
   {
    return(ClientServer_JsonDecoder.j())(JSON.parse(a));
   },serverReceive,function()
   {
    var b$1;
    b$1=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$2;
       b$2=null;
       return Concurrency.Delay(function()
       {
        var data;
        return msg.$==3?(writen(function($1)
        {
         return $1("WebSocket connection closed.");
        }),Concurrency.Return(state)):msg.$==2?(writen(function($1)
        {
         return $1("WebSocket connection open.");
        }),Concurrency.Return(state)):msg.$==1?(writen(function($1)
        {
         return $1("WebSocket connection error!");
        }),Concurrency.Return(state)):(data=msg.$0,Concurrency.Combine(data.$==3?(((writen(Runtime.Curried3(function($1,$2,$3)
        {
         return $1("MessageFromServer_String "+Utils.toSafe($2)+" \r\n(state: "+Global.String($3)+")");
        })))(data.$0))(state),Concurrency.Zero()):(writen(function($1)
        {
         return $1("invalidMessage");
        }),Concurrency.Zero()),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        })));
       });
      };
     }]);
    });
   }),function(a)
   {
    a.Post({
     $:3,
     $0:"kickOff"
    });
    return Concurrency.Zero();
   });
  })),null);
  container.HtmlProvider.SetAttribute(container.get_Body(),"id","console");
  return container;
 };
 Client.WS=function(endpoint)
 {
  var container,b;
  function writen(fmt)
  {
   return fmt(function(s)
   {
    container.Dom.appendChild(self.document.createTextNode(s+"\n"));
   });
  }
  container=Tags.Tags().NewTag("pre",[]);
  Concurrency.Start((b=null,Concurrency.Delay(function()
  {
   var r;
   writen(function($1)
   {
    return $1("Checking regression #4...");
   });
   $.ajax((r={},r.url="/ws.txt",r.method="GET",r.success=function(x)
   {
    return(writen(function($1)
    {
     return function($2)
     {
      return $1(Utils.toSafe($2));
     };
    }))(x);
   },r.error=function(a,a$1,e)
   {
    return(writen(function($1)
    {
     return function($2)
     {
      return $1("KO: "+Utils.toSafe($2)+".");
     };
    }))(e);
   },r));
   return Concurrency.Bind(WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((ClientServer_JsonEncoder.j())(a));
   },function(a)
   {
    return(ClientServer_JsonDecoder.j())(JSON.parse(a));
   },endpoint,function()
   {
    var b$1;
    b$1=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$2;
       b$2=null;
       return Concurrency.Delay(function()
       {
        var data;
        return msg.$==3?(writen(function($1)
        {
         return $1("WebSocket connection closed.");
        }),Concurrency.Return(state)):msg.$==2?(writen(function($1)
        {
         return $1("WebSocket connection open.");
        }),Concurrency.Return(state)):msg.$==1?(writen(function($1)
        {
         return $1("WebSocket connection error!");
        }),Concurrency.Return(state)):(data=msg.$0,Concurrency.Combine(data.$==0?(((writen(Runtime.Curried3(function($1,$2,$3)
        {
         return $1("Response2 "+Global.String($2)+" (state: "+Global.String($3)+")");
        })))(data.$0))(state),Concurrency.Zero()):data.$==2?((writen(function($1)
        {
         return function($2)
         {
          return $1("Resp3 "+GeneratedPrintf.p($2));
         };
        }))(data.$0),Concurrency.Zero()):data.$==3?((writen(function($1)
        {
         return function($2)
         {
          return $1("MessageFromServer_String "+Utils.prettyPrint($2));
         };
        }))(data.$0),Concurrency.Zero()):(((writen(Runtime.Curried3(function($1,$2,$3)
        {
         return $1("Response1 "+Utils.toSafe($2)+" (state: "+Global.String($3)+")");
        })))(data.$0))(state),Concurrency.Zero()),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        })));
       });
      };
     }]);
    });
   }),function(a)
   {
    var conn;
    conn=a.get_Connection();
    return Concurrency.While(function()
    {
     return true;
    },Concurrency.Delay(function()
    {
     return Concurrency.Bind(Concurrency.Sleep(1000),function()
     {
      conn.send(JSON.stringify((ClientServer_JsonEncoder.j())({
       $:2,
       $0:{
        name:{
         "first-name":"John00",
         LastName:"Doe"
        },
        age:42
       }
      })));
      return Concurrency.Zero();
     });
    }));
   });
  })),null);
  return container;
 };
 Client.fsiCmd=function()
 {
  var rvInput,rvHisCmd,filterResultFlattened,nScript,webSocket2,curPos,submit,hisCmd,vReversed,filterBox;
  rvInput=Var.Create$1("");
  rvHisCmd=Var.Create$1([]);
  filterResultFlattened=Var.Lens(Client.filterResult(),function(arr)
  {
   var reg;
   reg=new Global.RegExp(Client.filterKeyWord().c);
   return Strings.concat(",",Arrays.filter(function(s)
   {
    return reg.test(s);
   },arr));
  },function(n,s)
  {
   return n.concat([s]);
  });
  nScript=Var.Create$1("named script");
  webSocket2=Var.Create$1("http://localhost:8080/");
  curPos=Var.Create$1(0);
  submit=Submitter.CreateOption(rvInput.v);
  hisCmd=Submitter.CreateOption(rvHisCmd.v);
  vReversed=View.MapAsync(function(a)
  {
   var input,b;
   return a!=null&&a.$==1?(input=a.$0,(Var.Set(rvHisCmd,rvHisCmd.c.concat([input])),Var.Set(curPos,curPos.c+1),(new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.fsiExecute:-1840423385",[input]))):(b=null,Concurrency.Delay(function()
   {
    return Concurrency.Return("");
   }));
  },submit.view);
  View.MapAsync(function(a)
  {
   var v,b;
   if(a!=null&&a.$==1)
   {
    if(Arrays.length(a.$0)===0)
     return(new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.getHisCmds:-118046996",[]);
    else
     if(a!=null&&a.$==1)
      {
       v=a.$0;
       b=null;
       return Concurrency.Delay(function()
       {
        return Concurrency.Return(v);
       });
      }
     else
      throw new MatchFailureException.New("Client.fs",246,33);
   }
   else
    return(new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.getHisCmds:-118046996",[]);
  },hisCmd.view);
  filterBox=Doc.Input([AttrProxy.Create("id","fKW"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","1")],Client.filterKeyWord()).on("blur",function()
  {
   return $("#filteredResult").val(filterResultFlattened.RVal());
  });
  return Doc.Element("div",[],[Doc.Element("div",[],[Doc.Button("Send",[],function()
  {
   submit.Trigger();
  }),Doc.Button("Clear Console",[],function()
  {
   $("#console").empty();
  }),Doc.Button("Clear Command",[],function()
  {
   $("#fsiCmd").val("");
  }),Doc.Button("Last Command",[],function()
  {
   var b;
   Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.getHisCmd:-447555547",[]),function(a)
    {
     $("#fsiCmd").val(a);
     return Concurrency.Zero();
    });
   })),null);
  }),Doc.Button("Previous Command",[],function()
  {
   var b;
   Arrays.length(rvHisCmd.c)===0?Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.getHisCmds:-118046996",[]),function(a)
    {
     Var.Set(rvHisCmd,a);
     return Concurrency.Combine(Arrays.length(rvHisCmd.c)>0?(Var.Set(curPos,Arrays.length(rvHisCmd.c)-1),Concurrency.Zero()):Concurrency.Zero(),Concurrency.Delay(function()
     {
      Var.Set(rvInput,Arrays.get(rvHisCmd.c,curPos.c));
      return Concurrency.Zero();
     }));
    });
   })),null):(curPos.c===0?Var.Set(curPos,Arrays.length(rvHisCmd.c)-1):Var.Set(curPos,curPos.c-1),Var.Set(rvInput,Arrays.get(rvHisCmd.c,curPos.c)));
   hisCmd.Trigger();
  }),Doc.Button("Get Script",[],function()
  {
   var b;
   Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.getNamedScript:-1840423385",[Global.String($("#nScript").val())]),function(a)
    {
     $("#fsiCmd").val(a);
     return Concurrency.Zero();
    });
   })),null);
  }),Doc.Button("Save Script",[],function()
  {
   var b;
   Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.upsertNamedScript:2040152023",[Global.String($("#nScript").val()),Global.String($("#fsiCmd").val())]),function(a)
    {
     $("#fsiCmd").val(a);
     return Concurrency.Zero();
    });
   })),null);
  }),Doc.Button("List Script",[],function()
  {
   var b;
   Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.listNamedScripts:-118046996",[]),function(a)
    {
     var s;
     s=Arrays.fold(function(str,item)
     {
      return str!==""?str+"\r\n"+item:item;
     },"",a);
     $("#nScript").val("");
     $("#fsiCmd").val(s);
     return Concurrency.Zero();
    });
   })),null);
  }),Doc.Button("Clear Result Cache",[],function()
  {
   Var.Set(Client.filterResult(),[]);
  }),Doc.Element("br",[],[]),Doc.Button("ConnectTo",[],function()
  {
   Concurrency.Start(Client.Send3(Global.String($("#webSocket2").val())),null);
  }),Doc.Element("br",[],[]),Doc.InputArea([AttrProxy.Create("id","webSocket2"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","1")],webSocket2),Doc.Element("br",[],[]),filterBox,Doc.InputArea([AttrProxy.Create("id","nScript"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","1")],nScript),Doc.InputArea([AttrProxy.Create("id","fsiCmd"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","10"),AttrProxy.Create("value","printfn \"orz\"")],rvInput)]),Doc.Element("hr",[],[]),Doc.InputArea([AttrProxy.Create("id","filteredResult"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","10")],filterResultFlattened),Doc.Element("h4",[AttrProxy.Create("class","text-muted")],[Doc.TextNode("The server responded:")]),Doc.Element("div",[],[Doc.Element("h1",[],[Doc.TextView(vReversed)])])]);
 };
 Client.Send3=function(uri)
 {
  var b;
  Var.Set(Client.content(),"");
  Client.edpnt().c.get_Connection().close();
  Var.Set(Client.edpnt(),null);
  b=null;
  return Concurrency.Delay(function()
  {
   var console,cs;
   function writen(fmt)
   {
    return fmt(function(s)
    {
     var a;
     Var.Set(Client.filterResult(),Client.filterResult().c.concat([s+"\n"]));
     a=self.document.createTextNode(s+"\n");
     cs.append.apply(cs,[a]);
    });
   }
   $("#consoleWC").empty();
   console=Doc.InputArea([AttrProxy.Create("id","console"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","10")],Client.content());
   cs=$("#console");
   return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.getPort:1510873242",[uri]),function(a)
   {
    var b$1;
    Concurrency.Start((b$1=null,Concurrency.Delay(function()
    {
     return Concurrency.Bind(WithEncoding.ConnectStateful(function(a$1)
     {
      return JSON.stringify((ClientServer_JsonEncoder.j())(a$1));
     },function(a$1)
     {
      return(ClientServer_JsonDecoder.j())(JSON.parse(a$1));
     },a,function()
     {
      var b$2;
      b$2=null;
      return Concurrency.Delay(function()
      {
       return Concurrency.Return([0,function(state)
       {
        return function(msg)
        {
         var b$3;
         b$3=null;
         return Concurrency.Delay(function()
         {
          var data;
          return msg.$==3?(writen(function($1)
          {
           return $1("WebSocket connection closed.");
          }),Concurrency.Return(state)):msg.$==2?(writen(function($1)
          {
           return $1("WebSocket connection open.");
          }),Concurrency.Return(state)):msg.$==1?(writen(function($1)
          {
           return $1("WebSocket connection error!");
          }),Concurrency.Return(state)):(data=msg.$0,Concurrency.Combine(data.$==3?(((writen(Runtime.Curried3(function($1,$2,$3)
          {
           return $1("MessageFromServer_String "+Utils.toSafe($2)+" \r\n(state: "+Global.String($3)+")");
          })))(data.$0))(state),Concurrency.Zero()):(writen(function($1)
          {
           return $1("invalidMessage");
          }),Concurrency.Zero()),Concurrency.Delay(function()
          {
           return Concurrency.Return(state+1);
          })));
         });
        };
       }]);
      });
     }),function(a$1)
     {
      Var.Set(Client.edpnt(),a$1);
      a$1.Post({
       $:3,
       $0:"kickOff"
      });
      return Concurrency.Zero();
     });
    })),null);
    Doc.RunById("consoleWC",console);
    return Concurrency.Zero();
   });
  });
 };
 Client.edpnt=function()
 {
  SC$1.$cctor();
  return SC$1.edpnt;
 };
 Client.Send2=function(serverReceive)
 {
  var container,b;
  function writen(fmt)
  {
   return fmt(function(s)
   {
    var x;
    Var.Set(Client.filterResult(),Client.filterResult().c.concat([s+"\n"]));
    x=self.document.createTextNode(s+"\n");
    container.elt.appendChild(x);
   });
  }
  container=Doc.InputArea([AttrProxy.Create("id","console"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","10")],Client.content());
  Concurrency.Start((b=null,Concurrency.Delay(function()
  {
   return Concurrency.Bind(WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((ClientServer_JsonEncoder.j())(a));
   },function(a)
   {
    return(ClientServer_JsonDecoder.j())(JSON.parse(a));
   },serverReceive,function()
   {
    var b$1;
    b$1=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$2;
       b$2=null;
       return Concurrency.Delay(function()
       {
        var data;
        return msg.$==3?(writen(function($1)
        {
         return $1("WebSocket connection closed.");
        }),Concurrency.Return(state)):msg.$==2?(writen(function($1)
        {
         return $1("WebSocket connection open.");
        }),Concurrency.Return(state)):msg.$==1?(writen(function($1)
        {
         return $1("WebSocket connection error!");
        }),Concurrency.Return(state)):(data=msg.$0,Concurrency.Combine(data.$==3?(((writen(Runtime.Curried3(function($1,$2,$3)
        {
         return $1("MessageFromServer_String "+Utils.toSafe($2)+" \r\n(state: "+Global.String($3)+")");
        })))(data.$0))(state),Concurrency.Zero()):(writen(function($1)
        {
         return $1("invalidMessage");
        }),Concurrency.Zero()),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        })));
       });
      };
     }]);
    });
   }),function(a)
   {
    a.Post({
     $:3,
     $0:"kickOff"
    });
    return Concurrency.Zero();
   });
  })),null);
  return container;
 };
 Client.content=function()
 {
  SC$1.$cctor();
  return SC$1.content;
 };
 Client.filterKeyWord=function()
 {
  SC$1.$cctor();
  return SC$1.filterKeyWord;
 };
 Client.filterResult=function()
 {
  SC$1.$cctor();
  return SC$1.filterResult;
 };
 Client.m2=function()
 {
  var varTxt,vLength,vWords;
  varTxt=Var.Create$1("orz");
  vLength=View.Map(function(l)
  {
   return(function($1)
   {
    return function($2)
    {
     return $1("You entered "+Global.String($2)+" characters.");
    };
   }(Global.id))(l);
  },View.Map(Strings.length,varTxt.v));
  vWords=Doc.BindView(function(words)
  {
   return Doc.Concat(Arrays.map(function(w)
   {
    return Doc.Element("li",[],[Doc.TextNode(w)]);
   },words));
  },View.Map(function(s)
  {
   return Strings.SplitChars(s,[" "],0);
  },varTxt.v));
  return Doc.Element("div",[],[Doc.Element("div",[],[Doc.Input([],varTxt),Doc.TextView(vLength)]),Doc.Element("div",[],[Doc.TextNode("You entered the following words:"),Doc.Element("ul",[],[vWords])])]);
 };
 Client.Main=function()
 {
  var rvInput,submit,vReversed;
  rvInput=Var.Create$1("");
  submit=Submitter.CreateOption(rvInput.v);
  vReversed=View.MapAsync(function(a)
  {
   var b;
   return a!=null&&a.$==1?(new AjaxRemotingProvider.New()).Async("ClientServer:testFrom0.Server.DoSomething:-1840423385",[a.$0]):(b=null,Concurrency.Delay(function()
   {
    return Concurrency.Return("");
   }));
  },submit.view);
  Doc.RunById("navbar",Doc.Element("div",[],[Doc.TextNode("This goes into #main.")]));
  return Doc.Element("div",[],[Doc.Input([],rvInput),Doc.Button("Send",[],function()
  {
   submit.Trigger();
  }),Doc.Element("hr",[],[]),Doc.Element("h4",[AttrProxy.Create("class","text-muted")],[Doc.TextNode("The server responded:")]),Doc.Element("div",[AttrProxy.Create("class","jumbotron")],[Doc.Element("h1",[],[Doc.TextView(vReversed)])])]);
 };
 SC$1.$cctor=function()
 {
  SC$1.$cctor=Global.ignore;
  SC$1.filterResult=Var.Create$1([]);
  SC$1.filterKeyWord=Var.Create$1("");
  SC$1.content=Var.Create$1("");
  SC$1.edpnt=Var.CreateWaiting();
 };
 GeneratedPrintf.p=function($1)
 {
  return"{"+("FirstName = "+Utils.prettyPrint($1["first-name"]))+"; "+("LastName = "+Utils.prettyPrint($1.LastName))+"}";
 };
 ClientServer_JsonEncoder.j=function()
 {
  return ClientServer_JsonEncoder._v?ClientServer_JsonEncoder._v:ClientServer_JsonEncoder._v=(Provider.EncodeUnion(void 0,{
   cmd:3,
   age:2,
   "int":1,
   str:0
  },[["Request1",[["$0","str",Provider.EncodeArray(Provider.Id()),0]]],["Request2",[["$0","int",Provider.EncodeArray(Provider.Id()),0]]],["Req3",[[null,true,Provider.Id()]]],["MessageFromClient",[["$0","cmd",Provider.Id(),0]]]]))();
 };
 ClientServer_JsonDecoder.j=function()
 {
  return ClientServer_JsonDecoder._v?ClientServer_JsonDecoder._v:ClientServer_JsonDecoder._v=(Provider.DecodeUnion(void 0,"type",[["int",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["name",[["$0","value",Provider.Id(),0]]],["msgStr",[["$0","value",Provider.Id(),0]]]]))();
 };
}());
