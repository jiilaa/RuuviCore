using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Runtime;

namespace net.jommy.RuuviCore.Grains
{
    public class RuuviTagRegistry : Grain, IRuuviTagRegistry
    {
        private readonly IPersistentState<RuuviTagRegistryState> _ruuviTagRegistry;
        private IDisposable _timer;

        public RuuviTagRegistry([PersistentState(nameof(RuuviTagRegistryState), "RuuviStorage")]
            IPersistentState<RuuviTagRegistryState> ruuviTagRegistryState)
        {
            _ruuviTagRegistry = ruuviTagRegistryState;
        }

        public override Task OnActivateAsync()
        {
            // Save last seen times hourly, they are not that important to save each time they are updated
            _timer = RegisterTimer(SaveRegistryChanges, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
            
            return base.OnActivateAsync();
        }

        private async Task SaveRegistryChanges(object arg)
        {
            if (_ruuviTagRegistry.State.Dirty)
            {
                await SaveChanges();
            }
        }

        public override async Task OnDeactivateAsync()
        {
            _timer?.Dispose();
            await SaveChanges();
        }

        public async Task AddOrUpdate(string macAddress, string name)
        {
            if (_ruuviTagRegistry.State.RuuviTags.TryGetValue(macAddress, out var ruuviTagInfo))
            {
                ruuviTagInfo.Name = name;
                ruuviTagInfo.ModificationTime = DateTime.UtcNow;
            }
            else
            {
                _ruuviTagRegistry.State.RuuviTags[macAddress] = 
                    new RuuviTagInfo
                    {
                        Name = name, 
                        MacAddress = macAddress, 
                        ModificationTime = DateTime.UtcNow
                    };
            }

            await SaveChanges();
        }

        private async Task SaveChanges()
        {
            _ruuviTagRegistry.State.LastSaved = DateTime.UtcNow;
            _ruuviTagRegistry.State.Dirty = false;
            await _ruuviTagRegistry.WriteStateAsync();
        }

        public Task Refresh(string macAddress, DateTime? timestamp)
        {
            if (_ruuviTagRegistry.State.RuuviTags.TryGetValue(macAddress, out var ruuviTagInfo))
            {
                if (!timestamp.HasValue)
                {
                    ruuviTagInfo.LastSeen = DateTime.UtcNow;
                }
                else if (!ruuviTagInfo.LastSeen.HasValue || timestamp > ruuviTagInfo.LastSeen.Value)
                {
                    ruuviTagInfo.LastSeen = timestamp;
                }
            }
            else
            {
                _ruuviTagRegistry.State.RuuviTags[macAddress] = 
                    new RuuviTagInfo
                    {
                        MacAddress = macAddress,
                        Name = "Unknown",
                        ModificationTime = DateTime.UtcNow,
                        LastSeen = timestamp ?? DateTime.UtcNow
                    };
            }
            
            _ruuviTagRegistry.State.Dirty = false;
            return Task.CompletedTask;
        }

        public Task<List<RuuviTagInfo>> GetAll()
        {
            return Task.FromResult(_ruuviTagRegistry.State.RuuviTags.Values.ToList());
        }
    }
}