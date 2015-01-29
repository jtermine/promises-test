using Termine.Promises;
using Termine.Promises.Generics;
using Termine.Promises.NLogInstrumentation;

namespace TestConsumePromise
{
    public class TestPromise: Promise<TestPromiseWorkload>
    {
        public TestPromise(TestPromiseWorkload workload)
        {
            WithWorkload(workload);
        }

        public override void Init()
        {
            this.WithNLogInstrumentation();
            this.WithValidator("validate", Validate);
            this.WithExecutor("transform", Transform);
            this.WithExecutor("transformAgain", TransformAgain);
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