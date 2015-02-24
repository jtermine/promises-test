using System.Globalization;
using ServiceStack.Redis;
using Termine.Promises;
using Termine.Promises.Generics;
using Termine.Promises.NLogInstrumentation;

namespace TestConsumePromise.Collections
{
    public class NewCollectionPromise: Promise<NewCollectionWorkload>
    {
        public override void Init()
        {
            this.WithNLogInstrumentation();

            WithValidator("validate.newCollection", ValidateNewCollection);

            WithExecutor("buildCollectionName", BuildCollectionName);
            WithExecutor("buildRedisCollectionName", BuildRedisCollectionName);
            WithExecutor("createCollection", CreateCollection);
        }

        private void ValidateNewCollection(NewCollectionWorkload newCollectionWorkload)
        {
            if (string.IsNullOrEmpty(newCollectionWorkload.CollectionName))
            {
                Abort(new NullCollectionName());
                return;
            }

            if (!newCollectionWorkload.CollectionName.Contains("olrt:") &&
                !newCollectionWorkload.CollectionName.Contains("olrn:") &&
                !newCollectionWorkload.CollectionName.Contains("olrc:") &&
                !newCollectionWorkload.CollectionName.Contains("olri:")) return;
            
            Abort(new InvalidCollectionName(newCollectionWorkload.CollectionName));
        }

        private void BuildCollectionName(NewCollectionWorkload newCollectionWorkload)
        {
            newCollectionWorkload.CollectionName = newCollectionWorkload.CollectionName.ToLowerInvariant();

            // if (not public), prepend the applicationName

            if (newCollectionWorkload.Visibility == 1) return;

            newCollectionWorkload.CollectionName =
                string.Format("{0}.{1}", "app", newCollectionWorkload.CollectionName).ToLowerInvariant();
        }

        private void BuildRedisCollectionName(NewCollectionWorkload newCollectionWorkload)
        {
            newCollectionWorkload.CollectionName = string.Format("olrt:{0}", newCollectionWorkload.CollectionName);
        }

        private void CreateCollection(NewCollectionWorkload newCollectionWorkload)
        {
            using (var redisManager = new PooledRedisClientManager(new[] { "127.0.0.1:6379" }))
            using (var redis = redisManager.GetClient())
            {
                redis.SetEntryInHash(newCollectionWorkload.CollectionName, "visibility",
                    newCollectionWorkload.Visibility.ToString(CultureInfo.InvariantCulture));
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
                EventPublicMessage = "The collectionName cannot contain the characters 'olrt:', 'olrc:', 'olri:', or 'olrn:'.";
                EventPublicDetails = string.Format("The collectionName analyzed was > {0}", collectionName);
            }
        }
    }
}