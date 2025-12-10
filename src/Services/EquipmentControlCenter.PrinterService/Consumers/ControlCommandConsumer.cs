using EquipmentControlCenter.Shared.Messages;
using MassTransit;
using EquipmentControlCenter.PrinterService.Services;

namespace EquipmentControlCenter.PrinterService.Consumers;

public class ControlCommandConsumer : IConsumer<ControlCommand>
{
    private readonly ILogger<ControlCommandConsumer> _logger;
    private readonly ServiceStateManager _stateManager;
    private readonly PrinterControlExecutor _controlExecutor;

    public ControlCommandConsumer(
        ILogger<ControlCommandConsumer> logger,
        ServiceStateManager stateManager,
        PrinterControlExecutor controlExecutor)
    {
        _logger = logger;
        _stateManager = stateManager;
        _controlExecutor = controlExecutor;
    }

    public async Task Consume(ConsumeContext<ControlCommand> context)
    {
        var command = context.Message;
        var startTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Processing control command {CommandId} for control {ControlId}",
            command.CommandId,
            command.ControlId);

        try
        {
            var result = await _controlExecutor.ExecuteAsync(command.ControlId, command.Value);

            var response = new ControlCommandResponse
            {
                CommandId = command.CommandId,
                ServiceId = command.ServiceId,
                ControlId = command.ControlId,
                Success = true,
                ProcessedAt = DateTime.UtcNow,
                ResultValue = result,
                ProcessingTime = DateTime.UtcNow - startTime
            };

            await context.RespondAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing control command {CommandId}", command.CommandId);

            var response = new ControlCommandResponse
            {
                CommandId = command.CommandId,
                ServiceId = command.ServiceId,
                ControlId = command.ControlId,
                Success = false,
                ProcessedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message,
                ErrorCode = "EXECUTION_ERROR",
                ProcessingTime = DateTime.UtcNow - startTime
            };

            await context.RespondAsync(response);
        }
    }
}
