using Termine.Promises;
using Termine.Promises.ExectionControlWithRedis;
using Termine.Promises.Generics;
using Termine.Promises.Interfaces;
using Termine.Promises.NLogInstrumentation;

namespace TestConsumePromise
{
    public class TestPromise: Promise<TestPromiseWorkload>
    {
        public override void Init()
        {
            this.WithDuplicatePrevention();
            this.WithNLogInstrumentation();
            this.WithValidator("validate", Validate);
            this.WithExecutor("transform", Transform);
            this.WithExecutor("transformAgain", TransformAgain);
            this.WithSuccessHandler("reportSuccess", ReportSuccess);
        }

        private void ReportSuccess(IAmAPromise<TestPromiseWorkload> amAPromise)
        {
            Trace(new GenericEventMessage(0, amAPromise.Workload.Result));
        }

        private void Transform(TestPromiseWorkload testPromiseWorkload)
        {
            testPromiseWorkload.Result = string.Format("{0}-{1}", testPromiseWorkload.RequestId, "finished");
        }
        private void TransformAgain(TestPromiseWorkload testPromiseWorkload)
        {
            testPromiseWorkload.Result = string.Format("pre.{0}", testPromiseWorkload.Result);
        }

        private void Validate(TestPromiseWorkload testPromiseWorkload)
        {
            if (string.IsNullOrEmpty(testPromiseWorkload.RequestId)) Fatal(new GenericEventMessage(1, "RequestId is null or empty."));
        }
    }
}