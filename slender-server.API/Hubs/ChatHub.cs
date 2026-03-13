// slender-server.API/Hubs/ChatHub.cs
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
    IUserRepository userRepository) : Hub
{
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

    public async Task JoinChannel(string channelId, CancellationToken ct = default)
    {
        var userId = await GetUserIdAsync(ct);

        var accessResult = await channelService.GetByIdAsync(channelId, userId, ct);
        if (!accessResult.IsSuccess)
            throw new HubException("Access denied to channel.");

        await Groups.AddToGroupAsync(Context.ConnectionId, ChannelGroup(channelId), ct);
    }

    public async Task LeaveChannel(string channelId, CancellationToken ct = default)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChannelGroup(channelId), ct);
    }

    // ── Messaging ─────────────────────────────────────────────────────────────

    public async Task SendMessage(SendMessageRequest request, CancellationToken ct = default)
    {
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
            throw new HubException(result.Error ?? "Failed to send message.");

        await Clients
            .Group(ChannelGroup(request.ChannelId))
            .SendAsync("ReceiveMessage", result.Value, cancellationToken: ct);
    }

    public async Task EditMessage(EditMessageRequest request, CancellationToken ct = default)
    {
        var userId = await GetUserIdAsync(ct);

        var dto = new UpdateMessageDto { Body = request.Body };
        var result = await messageService.UpdateAsync(request.MessageId, userId, dto, ct);
        if (!result.IsSuccess)
            throw new HubException(result.Error ?? "Failed to edit message.");

        await Clients
            .Group(ChannelGroup(result.Value!.ChannelId))
            .SendAsync("MessageEdited", result.Value, cancellationToken: ct);
    }

    public async Task DeleteMessage(string messageId, CancellationToken ct = default)
    {
        var userId = await GetUserIdAsync(ct);

        var result = await messageService.DeleteAsync(messageId, userId, ct);
        if (!result.IsSuccess)
            throw new HubException(result.Error ?? "Failed to delete message.");

        await Clients
            .Group(ChannelGroup(result.Value!))
            .SendAsync("MessageDeleted", new { MessageId = messageId, ChannelId = result.Value }, cancellationToken: ct);
    }

    public async Task Typing(string channelId, CancellationToken ct = default)
    {
        var userId = await GetUserIdAsync(ct);

        await Clients
            .OthersInGroup(ChannelGroup(channelId))
            .SendAsync("UserTyping", new { UserId = userId, ChannelId = channelId }, cancellationToken: ct);
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