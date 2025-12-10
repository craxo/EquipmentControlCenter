using System;
using System.Threading;
using System.Threading.Tasks;
using EquipmentControlCenter.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EquipmentControlCenter.Desktop.Services;

/// <summary>
/// Sends control commands to equipment services
/// </summary>
public class ControlCommandService
{
    private readonly IRequestClient<ControlCommand> _client;
    private readonly ILogger<ControlCommandService> _logger;

    public ControlCommandService(
        IRequestClient<ControlCommand> client,
        ILogger<ControlCommandService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<ControlCommandResponse> SendCommandAsync(
        string serviceId,
        string controlId,
        object value,
        CancellationToken cancellationToken = default)
    {
        var command = new ControlCommand
        {
            ServiceId = serviceId,
            ControlId = controlId,
            CommandId = Guid.NewGuid().ToString(),
            Value = value,
            RequestedAt = DateTime.UtcNow,
            RequestedBy = Environment.UserName
        };

        _logger.LogInformation("Sending command {CommandId} to {ServiceId}/{ControlId}",
            command.CommandId, serviceId, controlId);

        try
        {
            var response = await _client.GetResponse<ControlCommandResponse>(command, cancellationToken);
            var result = response.Message;

            if (result.Success)
            {
                _logger.LogInformation("Command {CommandId} succeeded: {Result}",
                    command.CommandId, result.ResultValue);
            }
            else
            {
                _logger.LogWarning("Command {CommandId} failed: {Error}",
                    command.CommandId, result.ErrorMessage);
            }

            return result;
        }
        catch (RequestTimeoutException)
        {
            _logger.LogError("Command {CommandId} timed out", command.CommandId);
            throw new TimeoutException($"Command timed out after waiting for response");
        }
    }
}
