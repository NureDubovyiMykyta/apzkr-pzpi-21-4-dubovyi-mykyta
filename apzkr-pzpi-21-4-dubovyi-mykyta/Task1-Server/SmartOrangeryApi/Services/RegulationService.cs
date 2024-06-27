using SmartOrangeryApi.Models;
using SmartOrangeryApi.Models.Enums;
using System.Net.Sockets;
using System.Text;

namespace SmartOrangeryApi.Services
{
    public class RegulationService
    {
        private readonly OrangeryService _orangeryService;
        private readonly DeviceService _deviceService;

        public RegulationService(OrangeryService orangeryService, DeviceService deviceService)
        {
            _orangeryService = orangeryService;
            _deviceService = deviceService;
        }

        public async Task RegulateConditions(Sensor sensor)
        {
            var orangery = await _orangeryService.GetAsync(sensor.OrangeryId.ToString());
            if (orangery == null)
            {
                throw new KeyNotFoundException("Orangery not found");
            }

            switch (sensor.Type)
            {
                case SensorType.Temperature:
                    await HandleTemperatureRegulation(sensor, orangery);
                    break;
                case SensorType.Humidity:
                    await HandleHumidityRegulation(sensor, orangery);
                    break;
                case SensorType.Light:
                    await HandleLightRegulation(sensor, orangery);
                    break;
                case SensorType.CO2:
                    await HandleCO2Regulation(sensor, orangery);
                    break;
            }
        }

        private async Task HandleTemperatureRegulation(Sensor sensor, Orangery orangery)
        {
            if (sensor.LastValue < orangery.OptimalTemperature - 1)
            {
                await TurnDeviceOn(DeviceType.Heater, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Cooler, orangery.Id.ToString());
            }
            else if (sensor.LastValue > orangery.OptimalTemperature + 1)
            {
                await TurnDeviceOn(DeviceType.Cooler, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Heater, orangery.Id.ToString());
            }
            else
            {
                await TurnDeviceOff(DeviceType.Heater, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Cooler, orangery.Id.ToString());
            }
        }

        private async Task HandleHumidityRegulation(Sensor sensor, Orangery orangery)
        {
            if (sensor.LastValue < orangery.OptimalHumidity - 5)
            {
                await TurnDeviceOn(DeviceType.Humidifier, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Dehumidifier, orangery.Id.ToString());
            }
            else if (sensor.LastValue > orangery.OptimalHumidity + 5)
            {
                await TurnDeviceOn(DeviceType.Dehumidifier, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Humidifier, orangery.Id.ToString());
            }
            else
            {
                await TurnDeviceOff(DeviceType.Humidifier, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Dehumidifier, orangery.Id.ToString());
            }
        }

        private async Task HandleLightRegulation(Sensor sensor, Orangery orangery)
        {
            if (sensor.LastValue < orangery.OptimalLight - 10)
            {
                await TurnDeviceOn(DeviceType.Lamp, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Blinds, orangery.Id.ToString());
            }
            else if (sensor.LastValue > orangery.OptimalLight + 10)
            {
                await TurnDeviceOn(DeviceType.Blinds, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Lamp, orangery.Id.ToString());
            }
            else
            {
                await TurnDeviceOff(DeviceType.Lamp, orangery.Id.ToString());
                await TurnDeviceOff(DeviceType.Blinds, orangery.Id.ToString());
            }
        }

        private async Task HandleCO2Regulation(Sensor sensor, Orangery orangery)
        {
            if (sensor.LastValue > orangery.OptimalCO2 + 100)
            {
                await TurnDeviceOn(DeviceType.Ventilation, orangery.Id.ToString());
            }
            else
            {
                await TurnDeviceOff(DeviceType.Ventilation, orangery.Id.ToString());
            }
        }

        private async Task TurnDeviceOn(DeviceType deviceType, string orangeryId)
        {
            var device = await _deviceService.GetByTypeAndOrangeryIdAsync(deviceType, orangeryId);
            if (device != null)
            {
                // Send command to device
                await SendCommandToDevice(device, "turn_on");
                // Update device status in the database
                device.Status = "on";
                await _deviceService.UpdateAsync(device.Id.ToString(), device);
            }
        }

        private async Task TurnDeviceOff(DeviceType deviceType, string orangeryId)
        {
            var device = await _deviceService.GetByTypeAndOrangeryIdAsync(deviceType, orangeryId);
            if (device != null)
            {
                // Send command to device
                await SendCommandToDevice(device, "turn_off");
                // Update device status in the database
                device.Status = "off";
                await _deviceService.UpdateAsync(device.Id.ToString(), device);
            }
        }

        public async Task SendCommandToDevice(Device device, string command)
        {
            try
            {
                using (var client = new TcpClient(device.IpAddress, device.Port))
                {
                    var data = Encoding.UTF8.GetBytes(command);
                    var stream = client.GetStream();

                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("Sent: {0}", command);

                    data = new Byte[256];
                    var responseData = String.Empty;
                    var bytes = stream.Read(data, 0, data.Length);
                    responseData = Encoding.UTF8.GetString(data, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
    
}
