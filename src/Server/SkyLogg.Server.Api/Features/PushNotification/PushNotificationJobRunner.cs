using AdsPush;
using AdsPush.Abstraction;
using System.Collections.Concurrent;
using Hangfire.Server;
using System.Net;
using SkyLogg.Server.Api.Infrastructure.Services;

namespace SkyLogg.Server.Api.Features.PushNotification;

public partial class PushNotificationJobRunner
{
    [AutoInject] private AppDbContext dbContext = default!;
    [AutoInject] private IAdsPushSender adsPushSender = default!;
    [AutoInject] private ServerExceptionHandler serverExceptionHandler = default!;

    public async Task RequestPush(int[] pushNotificationSubscriptionIds,
        PushNotificationRequest request,
        PerformContext context = null!,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await dbContext.PushNotificationSubscriptions
            .Where(pns => pushNotificationSubscriptionIds.Contains(pns.Id))
            .ToArrayAsync(cancellationToken);

        var payload = new AdsPushBasicSendPayload()
        {
            Title = AdsPushText.CreateUsingString(request.Title ?? "SkyLogg push"),
            Detail = AdsPushText.CreateUsingString(request.Message ?? string.Empty)
        };

        if (string.IsNullOrEmpty(request.Action) is false)
        {
            payload.Parameters.Add("action", request.Action);
        }
        if (string.IsNullOrEmpty(request.PageUrl) is false)
        {
            payload.Parameters.Add("pageUrl", request.PageUrl);
        }

        int succeededItems = 0;
        int failedItems = 0;

        ConcurrentBag<(int problematicSubscriptionId, AdsPushErrorType? errorType, HttpStatusCode? responseStatusCode, Exception exp)> problems = [];

        await Parallel.ForEachAsync(subscriptions, parallelOptions: new()
        {
            MaxDegreeOfParallelism = 10,
            CancellationToken = cancellationToken
        }, async (subscription, cancellationToken) =>
        {
            try
            {
                var target = subscription.Platform is "browser" ? AdsPushTarget.BrowserAndPwa
                                    : subscription.Platform is "fcmV1" ? AdsPushTarget.Android
                                    : subscription.Platform is "apns" ? AdsPushTarget.Ios
                                    : throw new NotImplementedException();

                await adsPushSender.BasicSendAsync(target, subscription.PushChannel, payload, default);

                Interlocked.Increment(ref succeededItems); // Inside Parallel.ForEachAsync simple ++ wouldn't work
            }
            catch (Exception exp)
            {
                Interlocked.Increment(ref failedItems);
                var adsPushException = exp as AdsPushException;
                problems.Add((subscription.Id, adsPushException?.ErrorType, adsPushException?.HttpResponse?.StatusCode, exp));
            }
        });

        if (problems.IsEmpty is false)
        {
            var errorData = new Dictionary<string, object?>()
            {
                { "UserRelatedPush", request.UserRelatedPush },
                { "JobId", context.BackgroundJob.Id },
                { "TotalSubscriptions", pushNotificationSubscriptionIds.Length },
                { "SucceededItems", succeededItems },
                { "FailedItems", failedItems }
            };

            foreach (var (problematicSubscriptionId, errorType, responseStatusCode, exp) in problems.DistinctBy(p => new { p.errorType, p.responseStatusCode })) // DistinctBy to avoid huge error data in case of many same errors
            {
                errorData[$"Subscription_{problematicSubscriptionId}"] = $"ErrorType: {errorType}, ResponseStatusCode: {responseStatusCode}";
            }

            serverExceptionHandler.Handle(new AggregateException("Failed to send push notifications").WithData(errorData));
        }
    }
}
