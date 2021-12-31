﻿using System.Threading;
using System.Threading.Tasks;
using Bit.Core.Enums;
using Bit.Core.Models;
using Bit.Core.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace Bit.Notifications
{
    public static class HubHelpers
    {
        public static async Task SendNotificationToHubAsync(string notificationJson,
            IHubContext<NotificationsHub> hubContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var notification = JsonHelpers.Deserialize<PushNotificationData<object>>(notificationJson);
            switch (notification.Type)
            {
                case PushType.SyncCipherUpdate:
                case PushType.SyncCipherCreate:
                case PushType.SyncCipherDelete:
                case PushType.SyncLoginDelete:
                    var cipherNotification =
                        JsonHelpers.Deserialize<PushNotificationData<SyncCipherPushNotification>>(
                            notificationJson);
                    if (cipherNotification.Payload.UserId.HasValue)
                    {
                        await hubContext.Clients.User(cipherNotification.Payload.UserId.ToString())
                            .SendAsync("ReceiveMessage", cipherNotification, cancellationToken);
                    }
                    else if (cipherNotification.Payload.OrganizationId.HasValue)
                    {
                        await hubContext.Clients.Group(
                            $"Organization_{cipherNotification.Payload.OrganizationId}")
                            .SendAsync("ReceiveMessage", cipherNotification, cancellationToken);
                    }
                    break;
                case PushType.SyncFolderUpdate:
                case PushType.SyncFolderCreate:
                case PushType.SyncFolderDelete:
                    var folderNotification =
                        JsonHelpers.Deserialize<PushNotificationData<SyncFolderPushNotification>>(
                            notificationJson);
                    await hubContext.Clients.User(folderNotification.Payload.UserId.ToString())
                            .SendAsync("ReceiveMessage", folderNotification, cancellationToken);
                    break;
                case PushType.SyncCiphers:
                case PushType.SyncVault:
                case PushType.SyncOrgKeys:
                case PushType.SyncSettings:
                case PushType.LogOut:
                    var userNotification =
                        JsonHelpers.Deserialize<PushNotificationData<UserPushNotification>>(
                            notificationJson);
                    await hubContext.Clients.User(userNotification.Payload.UserId.ToString())
                            .SendAsync("ReceiveMessage", userNotification, cancellationToken);
                    break;
                case PushType.SyncSendCreate:
                case PushType.SyncSendUpdate:
                case PushType.SyncSendDelete:
                    var sendNotification =
                        JsonHelpers.Deserialize<PushNotificationData<SyncSendPushNotification>>(
                                notificationJson);
                    await hubContext.Clients.User(sendNotification.Payload.UserId.ToString())
                        .SendAsync("ReceiveMessage", sendNotification, cancellationToken);
                    break;
                default:
                    break;
            }
        }
    }
}
