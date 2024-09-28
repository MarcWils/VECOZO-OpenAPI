using MarcWils.Vecozo.Berichtuitwisseling;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BerichtuitwisselingSdk.Sample
{
    internal static class Program
    {
        static async Task Main()
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
                        Code = "10000001s", // AGB-code, UZOVI, ...
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
        }
    }
}
