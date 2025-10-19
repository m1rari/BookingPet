using BuildingBlocks.Common.Results;
using BuildingBlocks.EventBus;
using Booking.Application.Contracts;
using Booking.Application.IntegrationEvents;
using Booking.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Booking.Application.Sagas;

/// <summary>
/// Saga for coordinating the booking creation process across multiple services.
/// Steps: 1) Reserve resource → 2) Initiate payment → 3) Confirm booking
/// Implements compensation logic for rollback on failures.
/// </summary>
public class CreateBookingSaga
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateBookingSaga> _logger;

    public CreateBookingSaga(
        IBookingRepository bookingRepository,
        IEventBus eventBus,
        ILogger<CreateBookingSaga> logger)
    {
        _bookingRepository = bookingRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<Guid>> ExecuteAsync(
        Guid resourceId,
        Guid userId,
        DateTime startTime,
        DateTime endTime,
        decimal pricePerHour,
        CancellationToken cancellationToken = default)
    {
        var sagaId = Guid.NewGuid();
        _logger.LogInformation("Starting CreateBookingSaga {SagaId} for User {UserId}, Resource {ResourceId}",
            sagaId, userId, resourceId);

        Domain.Aggregates.Booking? booking = null;

        try
        {
            // Step 1: Create booking in Pending state
            var period = BookingPeriod.Create(startTime, endTime);
            var totalHours = period.CalculateTotalHours();
            var totalPrice = Money.Create(pricePerHour * totalHours);

            booking = Domain.Aggregates.Booking.Create(resourceId, userId, period, totalPrice);

            await _bookingRepository.AddAsync(booking, cancellationToken);
            await _bookingRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saga {SagaId}: Booking {BookingId} created in Pending state", 
                sagaId, booking.Id);

            // Step 2: Publish event to reserve resource in Inventory Service
            await _eventBus.PublishAsync(new ReserveResourceIntegrationEvent(
                booking.Id,
                resourceId,
                startTime,
                endTime
            ), cancellationToken);

            _logger.LogInformation("Saga {SagaId}: Published ReserveResourceIntegrationEvent", sagaId);

            // Step 3: Publish event to initiate payment in Payment Service
            await _eventBus.PublishAsync(new InitiatePaymentIntegrationEvent(
                booking.Id,
                userId,
                totalPrice.Amount,
                totalPrice.Currency
            ), cancellationToken);

            _logger.LogInformation("Saga {SagaId}: Published InitiatePaymentIntegrationEvent", sagaId);

            // Note: The saga will be completed by event handlers listening to:
            // - ResourceReservedIntegrationEvent (from Inventory)
            // - PaymentCompletedIntegrationEvent (from Payment)
            // When both are received, the booking will be confirmed

            return Result.Success(booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga {SagaId}: Error occurred, starting compensation", sagaId);

            // Compensating transactions
            await CompensateAsync(booking, sagaId, cancellationToken);

            return Result.Failure<Guid>(new Error(
                "CreateBookingSaga.Failed",
                $"Failed to create booking: {ex.Message}"));
        }
    }

    private async Task CompensateAsync(
        Domain.Aggregates.Booking? booking, 
        Guid sagaId, 
        CancellationToken cancellationToken)
    {
        _logger.LogWarning("Saga {SagaId}: Executing compensation", sagaId);

        if (booking != null)
        {
            try
            {
                // Mark booking as failed
                booking.MarkAsFailed();
                await _bookingRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                // Publish compensation events
                await _eventBus.PublishAsync(new ReleaseResourceIntegrationEvent(
                    booking.ResourceId,
                    booking.Period.StartTime,
                    booking.Period.EndTime
                ), cancellationToken);

                await _eventBus.PublishAsync(new CancelPaymentIntegrationEvent(
                    booking.Id
                ), cancellationToken);

                _logger.LogInformation("Saga {SagaId}: Compensation completed successfully", sagaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga {SagaId}: Compensation failed", sagaId);
                // In production, this should be handled by a dead-letter queue or manual intervention
            }
        }
    }
}

// Integration events for Saga coordination
public record ReserveResourceIntegrationEvent(
    Guid BookingId,
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime
) : IntegrationEvent;

public record InitiatePaymentIntegrationEvent(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency
) : IntegrationEvent;

public record ReleaseResourceIntegrationEvent(
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime
) : IntegrationEvent;

public record CancelPaymentIntegrationEvent(
    Guid BookingId
) : IntegrationEvent;


