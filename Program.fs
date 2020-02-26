open System.Net
open System.IO

let [<Literal>] statsGuru = "http://stats.espnscrum.com/statsguru/rugby/stats/index.html?class=1;filter=advanced;orderby=date;page=2;size=200;spanmax1=25+Feb+2020;spanmax2=26+Feb+2020;spanmin1=25+Feb+2015;spanmin2=26+Feb+2015;spanval1=span;spanval2=span;team=1;team=2;team=20;team=3;team=4;team=9;template=results;type=player;view=match"
type RugbyUnionPage = FSharp.Data.HtmlProvider<statsGuru>

let page n =
    "http://stats.espnscrum.com/statsguru/rugby/stats/index.html?" +
    sprintf "class=1;filter=advanced;orderby=date;page=%d;" n +
    "size=200;spanmax1=25+Feb+2020;spanmax2=26+Feb+2020;spanmin1=25+Feb+2015;" +
    "spanmin2=26+Feb+2015;spanval1=span;spanval2=span;team=1;team=2;team=20;team=3;team=4;team=9;" +
    "template=results;type=player;view=match"

let getSixNationsPlayerRecords() =
    [ 1..42 ] |> Seq.map (fun i ->
        async {
            let request = WebRequest.Create(page i)
            use! response = request.AsyncGetResponse()
            use reader = new StreamReader(response.GetResponseStream())
            let html = reader.ReadToEnd()
            let page = RugbyUnionPage.Parse(html)
            return page.Tables.``Match list``.Rows
        }
    ) |> Async.Parallel
    |> Async.RunSynchronously
    |> Array.concat

[<EntryPoint>]
let main argv =
    printf "date,opposition,ground,player,points,tries,penalties,conversions,goals\n"
    getSixNationsPlayerRecords() |> Array.iter (fun row ->
        printf "%A,%s,%s,%s,%d,%d,%d,%d,%d\n" row.``Match Date`` row.Opposition row.Ground row.Player row.Pts row.Tries row.Pens row.Conv row.Drop
    )
    0 // return an integer exit code
