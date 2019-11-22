using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Sitecore.ContentHub.Twitter.Models;
using Sitecore.ContentHub.Twitter.Utils;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Sitecore.ContentHub.Twitter
{
    public static class TweetContent
    {
        private const string DescFieldId = "Advertisement_Body";
        private const string ContentAssetRelation = "CmpContentToLinkedAsset";
        private const string AssetRenditionToUse = "downloadPreview";

        [FunctionName("TweetContent")]
        public static async Task Run([ServiceBusTrigger(
            "cmp_content",
            "twitter",
            Connection = "ServiceBus:ManageSendListenConnectionString")]
            ServiceBusMessageRequest chMessage,
            ILogger logger)
        {

            logger.LogDebug($"C# ServiceBus topic trigger function processed message. TargetId: {chMessage.Message.TargetId}");
            var contentId = chMessage.Message.TargetId;

            var contentEntity = await GetContentEntity(contentId, logger);

            if (contentEntity == null || !contentEntity.Id.HasValue)
            {
                logger.LogDebug($"contentEntity NOT found");
            }

            Auth.SetUserCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, AppSettings.TwitterUserAccessToken, AppSettings.TwitterUserAccessSecret);

            var tweetImage = await GetTwitterImage(contentEntity);
            var tweet = GetTweetContent(contentEntity, logger);

            if (tweetImage != null)
            {
                Tweet.PublishTweet(tweet, new PublishTweetOptionalParameters
                {
                    Medias = { tweetImage }
                });
            }
            else
            {
                Tweet.PublishTweet(tweet);
            }
            logger.LogDebug($"Tweet sent.");
        }

        private static async Task<IEntity> GetContentEntity(long entityId, ILogger logger)
        {
            var propertyLoad = new PropertyLoadOption(new string[] { DescFieldId });
            var relationLoad = new RelationLoadOption(new string[] { ContentAssetRelation });
            var loadConfig = new EntityLoadConfiguration(CultureLoadOption.Default, propertyLoad, relationLoad);
            IEntity entity = await MConnector.Client.Entities.GetAsync(entityId, loadConfig);
            return entity;
        }

        private static string GetTweetContent(IEntity content, ILogger logger)
        {
            var body = content.GetPropertyValue<string>(DescFieldId);
            var escapedBody = Regex.Replace(body, "<.*?>", string.Empty);
            logger.LogDebug($"body: {escapedBody}");
            return escapedBody;
        }

        private static async Task<IMedia> GetTwitterImage(IEntity content)
        {
            var relation = content.GetRelation<IParentToManyChildrenRelation>(ContentAssetRelation);
            if(relation != null)
            {
                var ids = relation.GetIds();
                if(ids != null && ids.Any())
                {
                    var asset = await MConnector.Client.Entities.GetAsync(ids.First()); //get first asset linked to the content entity
                    var downloadReviewRenditions = asset.Renditions.Where(x => x.Name.Equals(AssetRenditionToUse));
                    if(downloadReviewRenditions != null && downloadReviewRenditions.Any() && downloadReviewRenditions.First().Items!=null && downloadReviewRenditions.First().Items.Any())
                    {
                        var href = downloadReviewRenditions.First().Items.First().Href;
                        using (var webClient = new WebClient())
                        {
                            byte[] imageBytes = webClient.DownloadData(href);
                            var media = Upload.UploadBinary(imageBytes);
                            return media;
                        }
                    }
                }
            }

            return null;
        }
    }
}
