using MarcWils.Vecozo.VspKoppelingSdk.Berichtuitwisseling.Push.V1;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VspKoppelingSdk.Sample
{
    static class IndienenBerichtDemo
    {
        public static async Task IndienenBerichtAsync()
        {
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
        }
    }
}
