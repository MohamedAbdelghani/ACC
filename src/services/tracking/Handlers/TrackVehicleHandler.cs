﻿using ACC.Common.Exceptions;
using ACC.Common.Extensions;
using ACC.Common.Messaging;
using ACC.Services.Tracking.Commands;
using ACC.Services.Tracking.Domain;
using ACC.Services.Tracking.Events;
using ACC.Services.Tracking.Repositories;
using ACC.Services.Tracking.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ACC.Services.Tracking.Handlers
{
    public class TrackVehicleHandler : ICommandHandler<TrackVehicleCommand>

    {
        private readonly ITrackedVehicleRepository _trackedVehicleRepository;
        private readonly ICustomerService _customerService;
        private readonly IVehicleService _vehicleService;
        private readonly IEventBus _eventBus;
        private readonly ILogger _logger;

        public TrackVehicleHandler(ITrackedVehicleRepository trackedVehicleRepository,
            ICustomerService customerService,
            IVehicleService vehicleService,
            IEventBus eventBus,
            ILogger<TrackVehicleHandler> logger)
        {
            _trackedVehicleRepository = trackedVehicleRepository;
            _customerService = customerService;
            _vehicleService = vehicleService;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task HandleAsync(TrackVehicleCommand command)
        {
            var alreayTracked = await _trackedVehicleRepository.ExistsAsync(command.VehicleId)
                                  .AnyContext();

            var customerId = "";

            try
            {
                if (alreayTracked)
                {
                    throw new AccException("vehicle_already_tracked", $"Vehicle: '{command.VehicleId}' already tracked.");
                }

                var vehicle = await _vehicleService.GetAsync(command.VehicleId)
                                    .AnyContext();

                if (vehicle == null)
                {
                    throw new AccException("vehicle_not_found", $"Vehicle: '{command.VehicleId}' was not found");
                }
                customerId = vehicle.CustomerId;

                var customer = await _customerService.GetAsync(vehicle.CustomerId)
                        .AnyContext();

                if (customer == null)
                {
                    throw new AccException("customer_not_found", $"Customer: '{vehicle.CustomerId}' was not found");
                }

                var trackedVehicle = new TrackedVehicle(vehicle.Id, command.IPAddress, vehicle.RegNr, customer.Id.ToLower(), customer.Name, customer.Address);

                await _trackedVehicleRepository.AddAsync(trackedVehicle)
                      .AnyContext();

                await _eventBus.PublishAsync(new VehicleTrackedEvent(vehicle.Id))
                    .AnyContext();
            }
            catch (AccException ex)
            {
                await _eventBus.PublishAsync(new TrackVehicleRejectedEvent(command.VehicleId, customerId, ex.Code, ex.Message))
                                 .AnyContext();
            }
        }
    }
}