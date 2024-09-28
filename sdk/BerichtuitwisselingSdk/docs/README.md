# SDK voor VECOZO Berichtuitwisseling API's

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
        ConversatieId = Guid.NewGuid(),
        TraceerId = Guid.NewGuid()
    }
}, new FileParameter(new MemoryStream(Encoding.UTF8.GetBytes("Hello world!"))));
```

## Disclaimer
This is no official VECOZO NuGet package.