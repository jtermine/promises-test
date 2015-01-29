using System.Text;
using System.Web;
using Newtonsoft.Json;
using Termine.Promises.Web;

namespace TestConsumePromise
{
    public class TestPromiseHandler: PromisesHandler
    {
        public override void HandlePromise(byte[] body, WebConstants webConstants, HttpContext context)
        {
            if (webConstants != WebConstants.Json) return;

            var json = Encoding.UTF8.GetString(body);
            var workload = JsonConvert.DeserializeObject<TestPromiseWorkload>(json);

            var promise = new TestPromise(workload);

            promise.Run();

            context.Response.Write(promise.Workload.Result);
        }
    }
}