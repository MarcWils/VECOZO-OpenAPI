# SDK voor VECOZO VSP-Koppeling API's

## VSP-Koppeling berichtuitwisseling REST Push V1 sample

```csharp
var services = new ServiceCollection();

services.AddHttpClient<BerichtuitwisselingPushClient>(httpClient =>
    /*
        * Endpoints:
        * - TST: https://tst-api.vecozo.nl/tst/berichtenservice/berichten/rest-push/v1/
        * - ACC: https://tst-api.vecozo.nl/acc/berichtenservice/berichten/rest-push/v1/
        * - PRD: https://api.vecozo.nl/berichtenservice/berichten/rest-push/v1/
        * */
    httpClient.BaseAddress = new Uri("https://tst-api.vecozo.nl/tst/berichtenservice/berichten/rest-push/v1/")

).ConfigurePrimaryHttpMessageHandler(sp =>
{
    var handler = new HttpClientHandler();

    // Certicaat inladen van disk, certificate store, KeyVault, o.i.d.
    var certificate = new X509Certificate2("path-to-certificate.pfx", "pwd");
    handler.ClientCertificates.Add(certificate);
    return handler;
});

var sp = services.BuildServiceProvider();

var pushClient = sp.GetRequiredService<BerichtuitwisselingPushClient>();
await pushClient.PostBerichtAsync(new BerichtMetadata
{
    Routeringgegevens = new Routeringgegevens
    {
        Afzender = new Relatie
        {
            Rol = "Praktijk", // Praktijk, Zorgverlener, Instelling, Zorgverzekeraar, ...
            Code = "10000001", // AGB-code, UZOVI, ...
        },
        Geadresseerden = [
            new Relatie {
                Rol = "Zorgverzekeraar",
                Code = "1000"
            }
        ],
        Berichtstroom = new Berichtstroom
        {
            // Zie berichtuitwisselingsdocumentatie
            Berichttype = "Test",
            Berichtsubtype = "Test",
            Actie = "Indienen",
            Berichtversie = 1,
            Berichtsubversie = 0
        }
    },
    Referentiegegevens = new Referentiegegevens
    {
        // Conversatie ID koppelt meerdere berichten logisch aan elkaar.
        ConversatieId = Guid.NewGuid(),
        // Traceer ID is uniek voor elk bericht. Dit kan later gebruikt worden om de berichtstatus op te halen
        // of een retourbestand te correleren.
        TraceerId = Guid.NewGuid()
    }
},
// Data (payload) is het werkelijk uit te wissel bestand.
new FileParameter(data: new MemoryStream(Encoding.UTF8.GetBytes("Hello world!"))));
```

## VSP-Koppeling berichtuitwisseling REST Pull V1 sample

```csharp
var services = new ServiceCollection();

services.AddHttpClient<BerichtuitwisselingPullClient>(httpClient =>
    /*
        * Endpoints:
        * - TST: https://tst-api.vecozo.nl/tst/berichtenservice/berichten/rest-pull/v1/
        * - ACC: https://tst-api.vecozo.nl/acc/berichtenservice/berichten/rest-pull/v1/
        * - PRD: https://api.vecozo.nl/berichtenservice/berichten/rest-pull/v1/
        * */
    httpClient.BaseAddress = new Uri("https://tst-api.vecozo.nl/tst/berichtenservice/berichten/rest-pull/v1/")

).ConfigurePrimaryHttpMessageHandler(sp =>
{
    var handler = new HttpClientHandler();

    // Certicaat inladen van disk, certificate store, KeyVault, o.i.d.
    var certificate = new X509Certificate2("path-to-certificate.pfx", "pwd");
    handler.ClientCertificates.Add(certificate);
    return handler;
});

var sp = services.BuildServiceProvider();

var pullClient = sp.GetRequiredService<BerichtuitwisselingPullClient>();
var httpClient = sp.GetRequiredService<HttpClient>();

// Haal de 100 langst gereedstaande berichten op.
// Evt. kan er gerichter gezocht worden via de .ZoekBerichtenAsync() methode.
var berichten = await pullClient.GetBerichtenAsync(limit: 100);

foreach (var bericht in berichten)
{
    // bericht bevat alle metadata bij het bericht (berichtstroom, afzender, geadresseerde, e.d.).

    // De payload (bestand) dient opgehaald te worden via een aparte call.
    var payload = await httpClient.GetStreamAsync(bericht.Basisgegevens.Payload.Url);

    // Als de metadata en payload goed opgehaald (en geregistreerd is), dient de ontvangst bevestigd te worden.
    // Zo kan de volgende set aan gereedstaande berichten opgehaald worden.
    await pullClient.BevestigOntvangstAsync(bericht.Referentiegegevens.TraceerId);
}
```


## Disclaimer
This is no official VECOZO NuGet package.