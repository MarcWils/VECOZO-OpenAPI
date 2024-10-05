using MarcWils.Vecozo.VspKoppelingSdk.Berichtuitwisseling.Pull.V1;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace VspKoppelingSdk.Sample
{
    static class PullenBerichtenDemo
    {
        public static async Task PullBerichtenAsync()
        {
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
        }
    }
}
