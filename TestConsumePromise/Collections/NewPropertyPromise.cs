using System;
using Termine.Promises.Interfaces;

namespace TestConsumePromise.Collections
{
    public class NewPropertyPromise: IAmAPromiseFactory
    {
        public NewPropertyPromise()
        {
            
        }

        public string Run(string json)
        {
            throw new NotImplementedException();
        }
    }
}