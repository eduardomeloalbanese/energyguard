namespace EnergyGuard.Application.Interfaces;

public interface IDeviceQueryRepository
{
    Task<bool> SerialNumberExistsAsync(string serialNumber);
}
