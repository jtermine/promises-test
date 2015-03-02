using System.Globalization;
using System.Text.RegularExpressions;
using ServiceStack.Redis;
using Termine.Promises;
using Termine.Promises.Generics;
using Termine.Promises.Interfaces;
using Termine.Promises.NLogInstrumentation;

namespace TestConsumePromise.Collections
{
    public class NewCollectionFactory: IAmAPromiseFactory
    {
        readonly Promise<GenericConfig, NewCollectionWorkload, NewCollectionRequest, GenericResponse> _promise;

        public NewCollectionFactory()
        {
            _promise = new Promise<GenericConfig, NewCollectionWorkload, NewCollectionRequest, GenericResponse>();

            _promise.WithNLogInstrumentation()
                .WithValidator("validate.newCollection", ValidateNewCollection)
                .WithExecutor("buildCollectionName", BuildCollectionName)
                .WithExecutor("buildRedisCollectionName", BuildRedisCollectionName)
                .WithExecutor("createCollection", CreateCollection);
        }

        public string Run(string json)
        {
            return _promise.DeserializeRequest(json).Run().SerializeResponse();
        }

        private void ValidateNewCollection(IHandlePromiseActions handlePromiseActions, GenericConfig genericConfig, NewCollectionWorkload newCollectionWorkload, NewCollectionRequest request, GenericResponse arg5)
        {
            if (string.IsNullOrEmpty(request.CollectionName))
            {
                handlePromiseActions.Abort(new NullCollectionName());
                return;
            }

            if (Regex.Match(request.CollectionName, @"^|:olr.*:", RegexOptions.IgnoreCase).Success)
                handlePromiseActions.Abort(new InvalidCollectionName(request.CollectionName));
        }

        private void BuildCollectionName(IHandlePromiseActions handlePromiseActions, GenericConfig genericConfig, NewCollectionWorkload newCollectionWorkload, NewCollectionRequest request, GenericResponse arg5)
        {
            request.CollectionName = request.CollectionName.ToLowerInvariant();

            // if (not public), prepend the applicationName

            if (request .Visibility == 1) return;

            request.CollectionName =
                string.Format("{0}.{1}", "app", request.CollectionName).ToLowerInvariant();
        }

        private void BuildRedisCollectionName(IHandlePromiseActions handlePromiseActions, GenericConfig genericConfig, NewCollectionWorkload newCollectionWorkload, NewCollectionRequest request, GenericResponse arg5)
        {
            request.CollectionName = string.Format("olrt:{0}", request.CollectionName);
        }

        private void CreateCollection(IHandlePromiseActions handlePromiseActions, GenericConfig genericConfig, NewCollectionWorkload newCollectionWorkload, NewCollectionRequest request, GenericResponse arg5)
        {
            using (var redisManager = new PooledRedisClientManager(new[] { "127.0.0.1:6379" }))
            using (var redis = redisManager.GetClient())
            {
                redis.SetEntryInHash(request.CollectionName, "visibility",
                    request.Visibility.ToString(CultureInfo.InvariantCulture));
            }
        }

        private class NullCollectionName : GenericEventMessage
        {  
            public NullCollectionName()
            {
                EventId = 500;
                EventPublicMessage = "The request failed to provide a collectionName.";
            }
        }

        private class InvalidCollectionName : GenericEventMessage
        {
            public InvalidCollectionName(string collectionName)
            {
                EventId = 500;
                EventPublicMessage = "The collectionName cannot start with the characters 'olr*:' (* = wildcard).";
                EventPublicDetails = string.Format("The collectionName analyzed was > {0}", collectionName);
            }
        }
    }
}