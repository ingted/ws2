namespace testFrom0

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Server



type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About
    | [<EndPoint "/websocket">] WS
    | [<EndPoint "/fsi">] FSI
    | [<EndPoint "/fsiOld">] FSIOLD

module Templating =
    open WebSharper.UI.Next.Html

    type MainTemplate = Templating.Template<"Main.html">
    type FSTemplate = Templating.Template<"main2.html">
    type FSIndex = Templating.Template<"wwwroot/index.html">

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             liAttr [if endpoint = act then yield attr.``class`` "active"] [
                aAttr [attr.href (ctx.Link act)] [text txt]
             ]
        [
            "Home" => EndPoint.Home
            "About" => EndPoint.About
            "WebSocket" => EndPoint.WS
            "FSI" => EndPoint.FSI
            "FSIOLD" => EndPoint.FSIOLD
        ]

    let Main ctx endPoint (title: string) (body: Doc list) =
        Content.Page(
            MainTemplate()
                
                .Title(title)
                .MenuBar(MenuBar ctx endPoint)
                .Body(body)
                .Doc()
        )


    let FSI (ctx: Context<EndPoint>) endPoint (title: string) (main: Doc list) =
        Content.Page(
            FSIndex()
                .main(main)
                //.Title(title)
                //.Body(body)
                .Doc()
                //.Body(body)
                //.Doc()
        )

module Site =
    open WebSharper.UI.Next.Html
    open WebSharper.Owin
    //open WebSharper.Html.Server
    open WebSharper
    open WebSharper.Sitelets
    open WebSharper.Owin.WebSocket
    //open WebSharper.UI.Next.Client

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            h1Attr [] [text "Say Hi to the server!"]
            divAttr [] [client <@ Client.Main() @>]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1Attr [] [text "About"]
            pAttr [] [text "This is a template WebSharper client-server application."]
            divAttr [] [client <@ Client.m2() @>]
        ]

    let FSIOLDPage (ctx:Context<EndPoint>) =
        let d = Templating.FSTemplate().Doc()
        Templating.FSI ctx EndPoint.FSI "fsi" [
            h1Attr [] [text "fsi"]
            //divAttr [] [client <@ Client.fsi() @>]
            d
        ]
    
    //let a =
    //    WebSharper.UI.Client.HtmlExtensions.on.readyStateChange (fun el ev ->
    //        ()
    //    )

    let FSIPage serverSend serverReceive ctx =
        let docList = Templating.MenuBar ctx EndPoint.FSI 
        let ws = ClientSide <@ Client.Send serverReceive @>

        let wc = 
            divAttr [] [
                
                divAttr [] [client <@ Client.fsiCmd () @>]
                divAttr [
                    Attr.Create "id" "fsiResult" 
                ] [
                    divAttr [] [Doc.WebControl ws] 
                ]
            ]
        Content.Page(
            Templating.MainTemplate()
                .Title("wsInSuave")
                .MenuBar(docList)
                .Body(wc)
                .Doc()
        )

    let Socketing send receive ctx =
        let docList = Templating.MenuBar ctx EndPoint.WS 
        let url = 
            //let b = Suave.Web.defaultConfig.bindings |> List.item 0
            //b.ToString()
            "http://localhost:8080"
        //let ep = WebSocket.Endpoint.Create(url, "/WS", JsonEncoding.Readable)
        let ws = ClientSide <@ Client.WS send @>
        let ws2 = ClientSide <@ Client.WS receive @>
        let wc = 
            divAttr [] [
                divAttr [] [Doc.WebControl ws; Doc.WebControl ws2]
            ]
        Content.Page(
            Templating.MainTemplate()
                .Title("wsInSuave")
                .MenuBar(docList)
                .Body(wc)
                .Doc()
        )
        //Templating.Main ctx EndPoint.WS "wsInSuave" [
             
        //]

    //let WSSitelet ep =
    //    Sitelet.Sum [
    //        Sitelet.Content "/WS" EndPoint.WS (Socketing ep)
    //    ]

    [<Website>]
    let Main serverSend serverReceive =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
            | EndPoint.WS -> Socketing serverSend serverReceive ctx
            | EndPoint.FSI -> FSIPage serverSend serverReceive ctx
            | EndPoint.FSIOLD -> FSIOLDPage ctx
        )
    //let s = Sitelet.New
