// slender-server.API/Hubs/ChatHub.cs
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Domain.Interfaces;

namespace slender_server.API.Hubs;

/// <summary>
/// SignalR hub for real-time chat messaging.
/// Clients join channel groups when they open a conversation.
/// </summary>
[Authorize]
public sealed class ChatHub(
    IMessageService messageService,
    IChannelService channelService,
    ILogger<ChatHub> logger,
    IUserRepository userRepository) : Hub
{
    // callId -> channelId mapping so we can route WebRTC signalling
    private static readonly ConcurrentDictionary<string, string> CallChannelMap = new();

    // ── Auth helper ───────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the application User.Id from the JWT NameIdentifier claim.
    /// IUserContext cannot be used here — it reads from IHttpContextAccessor.HttpContext
    /// which is null inside SignalR hub method invocations.
    /// </summary>
    private async Task<string> GetUserIdAsync(CancellationToken ct)
    {
        var identityId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityId))
            throw new HubException("Not authenticated.");

        var userId = await userRepository.GetIdByIdentityIdAsync(identityId, ct);
        if (string.IsNullOrEmpty(userId))
            throw new HubException("User not found.");

        return userId;
    }

    // ── Connection lifecycle ──────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    // ── Group management ─────────────────────────────────────────────────────

    public async Task JoinChannel(string channelId)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);
        var accessResult = await channelService.GetByIdAsync(channelId, userId, ct);
        if (!accessResult.IsSuccess)
            throw new HubException($"Access denied to channel: {accessResult.ErrorMessage}");
        await Groups.AddToGroupAsync(Context.ConnectionId, ChannelGroup(channelId), ct);
    }

    public async Task LeaveChannel(string channelId)
    {
        var ct = Context.ConnectionAborted;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChannelGroup(channelId), ct);
    }

    // ── Messaging ─────────────────────────────────────────────────────────────

    public async Task SendMessage(SendMessageRequest request)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        var dto = new CreateMessageDto
        {
            ChannelId = request.ChannelId,
            AuthorId = userId,
            Body = request.Body,
            ReplyToId = request.ReplyToId
        };
        var result = await messageService.CreateAsync(userId, dto, ct);
        if (!result.IsSuccess)
            throw new HubException(result.ErrorMessage ?? "Failed to send message.");
        
        logger.LogDebug("Message sent to channel {ChannelId} by user {UserId}", request.ChannelId, userId);

        await Clients
            .Group(ChannelGroup(request.ChannelId))
            .SendAsync("ReceiveMessage", result.Value, cancellationToken: ct);
    }

    public async Task EditMessage(EditMessageRequest request)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        var dto = new UpdateMessageDto { Body = request.Body };
        var result = await messageService.UpdateAsync(request.MessageId, userId, dto, ct);
        if (!result.IsSuccess)
            throw new HubException(result.ErrorMessage ?? "Failed to edit message.");

        await Clients
            .Group(ChannelGroup(result.Value!.ChannelId))
            .SendAsync("MessageEdited", result.Value, cancellationToken: ct);
    }

    public async Task DeleteMessage(string messageId)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        var result = await messageService.DeleteAsync(messageId, userId, ct);
        if (!result.IsSuccess)
            throw new HubException(result.ErrorMessage ?? "Failed to delete message.");

        await Clients
            .Group(ChannelGroup(result.Value!))
            .SendAsync("MessageDeleted", new { MessageId = messageId, ChannelId = result.Value }, cancellationToken: ct);
    }

    public async Task Typing(string channelId)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        await Clients
            .OthersInGroup(ChannelGroup(channelId))
            .SendAsync("UserTyping", new { UserId = userId, ChannelId = channelId }, cancellationToken: ct);
    }

    // ── 1:1 call signalling (audio / video via WebRTC) ────────────────────────

    public async Task StartCall(StartCallRequest request)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        if (!string.Equals(userId, request.FromUserId, StringComparison.Ordinal))
            throw new HubException("Caller mismatch.");

        // Ensure caller has access to the channel
        var accessResult = await channelService.GetByIdAsync(request.ChannelId, userId, ct);
        if (!accessResult.IsSuccess)
            throw new HubException($"Access denied to channel: {accessResult.ErrorMessage}");

        // Track which channel this call belongs to for later signalling
        CallChannelMap[request.CallId] = request.ChannelId;

        // Notify all participants in the channel; clients will filter by ToUserId.
        await Clients
            .Group(ChannelGroup(request.ChannelId))
            .SendAsync("IncomingCall", new
            {
                request.CallId,
                request.ChannelId,
                request.FromUserId,
                request.ToUserId,
                request.Kind
            }, cancellationToken: ct);

        // Also forward initial SDP offer as a CallSignal event.
        await Clients
            .Group(ChannelGroup(request.ChannelId))
            .SendAsync("CallSignal", new
            {
                request.CallId,
                request.FromUserId,
                request.ToUserId,
                Type = "offer",
                Data = request.Sdp
            }, cancellationToken: ct);
    }

    public async Task AcceptCall(AcceptCallRequest request)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        if (!CallChannelMap.TryGetValue(request.CallId, out var channelId))
            return;

        // Optional: broadcast acceptance so UI can update if needed
        await Clients
            .Group(ChannelGroup(channelId))
            .SendAsync("CallAccepted", new
            {
                request.CallId,
                ChannelId = channelId,
                AcceptedByUserId = userId
            }, cancellationToken: ct);
    }

    public async Task RejectCall(RejectCallRequest request)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        if (!CallChannelMap.TryGetValue(request.CallId, out var channelId))
            return;

        CallChannelMap.TryRemove(request.CallId, out _);

        await Clients
            .Group(ChannelGroup(channelId))
            .SendAsync("CallEnded", new
            {
                request.CallId,
                ChannelId = channelId,
                EndedByUserId = userId,
                Reason = "rejected"
            }, cancellationToken: ct);
    }

    public async Task EndCall(EndCallRequest request)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        if (!CallChannelMap.TryGetValue(request.CallId, out var channelId))
            return;

        CallChannelMap.TryRemove(request.CallId, out _);

        await Clients
            .Group(ChannelGroup(channelId))
            .SendAsync("CallEnded", new
            {
                request.CallId,
                ChannelId = channelId,
                EndedByUserId = userId,
                Reason = "ended"
            }, cancellationToken: ct);
    }

    public async Task CallSignal(CallSignalRequest request)
    {
        var ct = Context.ConnectionAborted;
        var userId = await GetUserIdAsync(ct);

        if (!CallChannelMap.TryGetValue(request.CallId, out var channelId))
            return;

        await Clients
            .Group(ChannelGroup(channelId))
            .SendAsync("CallSignal", new
            {
                request.CallId,
                FromUserId = userId,
                request.ToUserId,
                request.Type,
                request.Data
            }, cancellationToken: ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ChannelGroup(string channelId) => $"channel:{channelId}";
}

public sealed record SendMessageRequest(
    string ChannelId,
    string Body,
    string? ReplyToId = null);

public sealed record EditMessageRequest(
    string MessageId,
    string Body);

public sealed record StartCallRequest(
    string CallId,
    string ChannelId,
    string FromUserId,
    string ToUserId,
    string Kind,
    object Sdp);

public sealed record AcceptCallRequest(
    string CallId,
    string ChannelId);

public sealed record RejectCallRequest(
    string CallId,
    string ChannelId);

public sealed record EndCallRequest(
    string CallId,
    string ChannelId);

public sealed record CallSignalRequest(
    string CallId,
    string ToUserId,
    string Type,
    object Data);