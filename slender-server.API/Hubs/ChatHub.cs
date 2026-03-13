using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Interfaces.Services;

namespace slender_server.API.Hubs;

/// <summary>
/// SignalR hub for real-time chat messaging.
/// Clients join channel groups when they open a conversation.
/// </summary>
[Authorize]
public sealed class ChatHub(
    IMessageService messageService,
    IChannelService channelService,
    IUserContext userContext) : Hub
{
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

    /// <summary>
    /// Subscribe to real-time messages in a specific channel.
    /// </summary>
    public async Task JoinChannel(string channelId, CancellationToken ct = default)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);

        // Verify user has access to this channel
        var accessResult = await channelService.GetByIdAsync(channelId, userId, ct);
        if (!accessResult.IsSuccess)
        {
            throw new HubException("Access denied to channel.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ChannelGroup(channelId), ct);
    }

    /// <summary>
    /// Unsubscribe from a channel group.
    /// </summary>
    public async Task LeaveChannel(string channelId, CancellationToken ct = default)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChannelGroup(channelId), ct);
    }

    // ── Messaging ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Send a message to a channel. Persists to DB then broadcasts to group.
    /// </summary>
    public async Task SendMessage(SendMessageRequest request, CancellationToken ct = default)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);

        var dto = new CreateMessageDto
        {
            ChannelId = request.ChannelId,
            AuthorId = userId,
            Body = request.Body,
            ReplyToId = request.ReplyToId
        };

        var result = await messageService.CreateAsync(userId, dto, ct);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error ?? "Failed to send message.");
        }

        // Broadcast to everyone in the channel group (including sender for confirmation)
        await Clients
            .Group(ChannelGroup(request.ChannelId))
            .SendAsync("ReceiveMessage", result.Value, cancellationToken: ct);
    }

    /// <summary>
    /// Edit an existing message. Only the author can edit.
    /// </summary>
    public async Task EditMessage(EditMessageRequest request, CancellationToken ct = default)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);

        var dto = new UpdateMessageDto { Body = request.Body };
        var result = await messageService.UpdateAsync(request.MessageId, userId, dto, ct);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error ?? "Failed to edit message.");
        }

        await Clients
            .Group(ChannelGroup(result.Value!.ChannelId))
            .SendAsync("MessageEdited", result.Value, cancellationToken: ct);
    }

    /// <summary>
    /// Delete a message. Only the author can delete.
    /// </summary>
    public async Task DeleteMessage(string messageId, CancellationToken ct = default)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);

        var result = await messageService.DeleteAsync(messageId, userId, ct);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error ?? "Failed to delete message.");
        }

        // result.Value contains the channelId we deleted from
        await Clients
            .Group(ChannelGroup(result.Value!))
            .SendAsync("MessageDeleted", new { MessageId = messageId, ChannelId = result.Value }, cancellationToken: ct);
    }

    /// <summary>
    /// Broadcast typing indicator to other channel members.
    /// </summary>
    public async Task Typing(string channelId, CancellationToken ct = default)
    {
        var userId = await userContext.GetRequiredUserIdAsync(ct);

        await Clients
            .OthersInGroup(ChannelGroup(channelId))
            .SendAsync("UserTyping", new { UserId = userId, ChannelId = channelId }, cancellationToken: ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ChannelGroup(string channelId) => $"channel:{channelId}";
}

// ── Request records ───────────────────────────────────────────────────────────

public sealed record SendMessageRequest(
    string ChannelId,
    string Body,
    string? ReplyToId = null);

public sealed record EditMessageRequest(
    string MessageId,
    string Body);