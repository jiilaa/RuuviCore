using System.Threading.Tasks;
using Orleans;

namespace net.jommy.RuuviCore.Interfaces;

public interface ISimulatedTag : IGrainWithStringKey
{
    Task Start();
}