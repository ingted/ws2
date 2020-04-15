(function()
{
 "use strict";
 var Global,testFrom0,Client,WebSharper$Owin$WebSocket$Test_JsonEncoder,WebSharper$Owin$WebSocket$Test_JsonDecoder,GeneratedPrintf,WebSharper,Html,Client$1,Tags,Concurrency,$,Utils,Owin,WebSocket,Client$2,WithEncoding,JSON,IntelliFactory,Runtime,UI,Next,Doc,Var,View,Strings,Arrays,Submitter,Remoting,AjaxRemotingProvider,AttrProxy,ClientSideJson,Provider;
 Global=self;
 testFrom0=Global.testFrom0=Global.testFrom0||{};
 Client=testFrom0.Client=testFrom0.Client||{};
 WebSharper$Owin$WebSocket$Test_JsonEncoder=Global.WebSharper$Owin$WebSocket$Test_JsonEncoder=Global.WebSharper$Owin$WebSocket$Test_JsonEncoder||{};
 WebSharper$Owin$WebSocket$Test_JsonDecoder=Global.WebSharper$Owin$WebSocket$Test_JsonDecoder=Global.WebSharper$Owin$WebSocket$Test_JsonDecoder||{};
 GeneratedPrintf=Global.GeneratedPrintf=Global.GeneratedPrintf||{};
 WebSharper=Global.WebSharper;
 Html=WebSharper&&WebSharper.Html;
 Client$1=Html&&Html.Client;
 Tags=Client$1&&Client$1.Tags;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 $=Global.jQuery;
 Utils=WebSharper&&WebSharper.Utils;
 Owin=WebSharper&&WebSharper.Owin;
 WebSocket=Owin&&Owin.WebSocket;
 Client$2=WebSocket&&WebSocket.Client;
 WithEncoding=Client$2&&Client$2.WithEncoding;
 JSON=Global.JSON;
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 UI=WebSharper&&WebSharper.UI;
 Next=UI&&UI.Next;
 Doc=Next&&Next.Doc;
 Var=Next&&Next.Var;
 View=Next&&Next.View;
 Strings=WebSharper&&WebSharper.Strings;
 Arrays=WebSharper&&WebSharper.Arrays;
 Submitter=Next&&Next.Submitter;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 AttrProxy=Next&&Next.AttrProxy;
 ClientSideJson=WebSharper&&WebSharper.ClientSideJson;
 Provider=ClientSideJson&&ClientSideJson.Provider;
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
    return JSON.stringify((WebSharper$Owin$WebSocket$Test_JsonEncoder.j())(a));
   },function(a)
   {
    return(WebSharper$Owin$WebSocket$Test_JsonDecoder.j())(JSON.parse(a));
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
    return Concurrency.While(function()
    {
     return true;
    },Concurrency.Delay(function()
    {
     return Concurrency.Bind(Concurrency.Sleep(1000),function()
     {
      a.Post({
       $:2,
       $0:{
        name:{
         "first-name":"John",
         LastName:"Doe"
        },
        age:42
       }
      });
      return Concurrency.Zero();
     });
    }));
   });
  })),null);
  return container;
 };
 Client.fsi=function(doc)
 {
  Doc.RunById("main",doc);
  return Doc.Empty();
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
   return a!=null&&a.$==1?(new AjaxRemotingProvider.New()).Async("WebSharper.Owin.WebSocket.Test:testFrom0.Server.DoSomething:-1840423385",[a.$0]):(b=null,Concurrency.Delay(function()
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
 WebSharper$Owin$WebSocket$Test_JsonEncoder.j=function()
 {
  return WebSharper$Owin$WebSocket$Test_JsonEncoder._v?WebSharper$Owin$WebSocket$Test_JsonEncoder._v:WebSharper$Owin$WebSocket$Test_JsonEncoder._v=(Provider.EncodeUnion(void 0,{
   age:2,
   "int":1,
   str:0
  },[["Request1",[["$0","str",Provider.EncodeArray(Provider.Id()),0]]],["Request2",[["$0","int",Provider.EncodeArray(Provider.Id()),0]]],["Req3",[[null,true,Provider.Id()]]]]))();
 };
 WebSharper$Owin$WebSocket$Test_JsonDecoder.j=function()
 {
  return WebSharper$Owin$WebSocket$Test_JsonDecoder._v?WebSharper$Owin$WebSocket$Test_JsonDecoder._v:WebSharper$Owin$WebSocket$Test_JsonDecoder._v=(Provider.DecodeUnion(void 0,"type",[["int",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["name",[["$0","value",Provider.Id(),0]]]]))();
 };
 GeneratedPrintf.p=function($1)
 {
  return"{"+("FirstName = "+Utils.prettyPrint($1["first-name"]))+"; "+("LastName = "+Utils.prettyPrint($1.LastName))+"}";
 };
}());
