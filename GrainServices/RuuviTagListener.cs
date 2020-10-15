﻿using System;
using System.Threading.Tasks;
using bluez.DBus;
using Microsoft.Extensions.Logging;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Interfaces;
using Orleans;

namespace net.jommy.RuuviCore.GrainServices
{
    public class RuuviTagListener : AbstractDeviceListener
    {
        private readonly ILogger<RuuviTagListener> _logger;
        private const ushort RuuviManufacturerDataKey = 1177;
        private readonly Guid _grainAddress;

        protected override ushort ManufacturerKey => RuuviManufacturerDataKey;

        public RuuviTagListener(IDevice1 device, string deviceAddress, IGrainFactory grainFactory, ILogger<RuuviTagListener> logger)
            :base(device, deviceAddress, grainFactory)
        {
            _logger = logger;
            _grainAddress = deviceAddress.ToActorGuid();
        }

        protected override async Task HandlePropertiesChanged(byte[] manufacturerData)
        {
            await GrainFactory.GetGrain<IRuuviStreamWorker>(0).Publish(
                DeviceAddress,
                new MeasurementEnvelope
                {
                    MacAddress = DeviceAddress,
                    Timestamp = DateTime.UtcNow,
                    SignalStrength = await GetSignalStrength(),
                    Data = manufacturerData
                });
        }

        protected override async Task OnStartListening()
        {
            var ruuviTag = GrainFactory.GetGrain<IRuuviTag>(_grainAddress);
            var name = await ruuviTag.GetName();
            if (name != null)
            {
                _logger.LogInformation("RuuviTag {name} ({mac}) found.", name, DeviceAddress);
            }
            else
            {
                _logger.LogInformation("New RuuviTag {alias} found.", DeviceAddress);
            }
        }
    }
}
