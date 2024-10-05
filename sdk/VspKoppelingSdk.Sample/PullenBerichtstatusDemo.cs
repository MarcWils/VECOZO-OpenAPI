using MarcWils.Vecozo.VspKoppelingSdk.Berichtstatus.Pull.V2;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace VspKoppelingSdk.Sample
{
    internal static class PullenBerichtstatusDemo
    {
        public static async Task PullBerichtstatusAsync()
        {
            var services = new ServiceCollection();
            services.AddHttpClient<BerichtstatusPullClient>(httpClient =>
                /*
                 * Endpoints:
                 * - TST: https://tst-api.vecozo.nl/tst/berichtenservice/berichtstatus/rest-pull/v2/
                 * - ACC: https://tst-api.vecozo.nl/acc/berichtenservice/berichtstatus/rest-pull/v2/
                 * - PRD: https://api.vecozo.nl/berichtenservice/berichtstatus/rest-pull/v2/
                 * */
                httpClient.BaseAddress = new Uri("https://tst-api.vecozo.nl/tst/berichtenservice/berichtstatus/rest-pull/v2/")

            ).ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var handler = new HttpClientHandler();

                // Certicaat inladen van disk, certificate store, KeyVault, o.i.d.
                var certificate = new X509Certificate2("path-to-certificate.pfx", "pwd");
                handler.ClientCertificates.Add(certificate);
                return handler;
            });

            var sp = services.BuildServiceProvider();
            var berichtstatusPullClient = sp.GetRequiredService<BerichtstatusPullClient>();

            // De berichstatus kan opgehaald worden o.b.v. traceer ID of conversatie ID.
            var berichtstatussen = await berichtstatusPullClient.ZoekBerichtstatussenAsync(traceerId: Guid.NewGuid(), null);

            // Ook de de berichtstatus van de gekoppelde berichten (bv. retourberichten) worden teruggekoppeld.
            // Als het bericht is afgekeurd, worden ook de bijhorende meldingen teruggekoppeld.
        }
    }
}
