# RuuviCore - a complete RuuviTag data processing system
[![Build Status](https://dev.azure.com/jiilaa/RuuviCore/_apis/build/status/jiilaa.RuuviCore?branchName=main)](https://dev.azure.com/jiilaa/RuuviCore/_build/latest?definitionId=1&branchName=main)

This project is a .NET Core implementation on Orleans framework, with a purpose to listen, collect and deliver measurements received from 
RuuviTag (https://ruuvi.com/) environment sensors. Target platform is Linux (Raspberry Pi), 
as it utilizes DBus to receive Bluetooth low energy broadcasts.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

* Raspberry Pi 3 or newer (support for Bluetooth LE needed). Tested with RPi 3
* TBD


### Installing

A step by step series of examples that tell you how to get a development env running

TBD

## Deployment

TBD


## Contributing

Please read [CONTRIBUTING.md] for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Jomi Laakkonen** - *Initial baseline* - [jiilaa](https://github.com/jiilaa)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details


## In short
This project is written using .NET Core (currently version 3.1), utilizing actor model running on 
Orleans framework (https://dotnet.github.io/orleans/), which enables every registered RuuviTag device to be configured separately to 
do different things with the received data. RuuviCore subscribes to DBUS (using the Bluez stack), 
detects the nearby RuuviTag sensors by the manufacturer code, and sends the raw bytes to the correct actor (think of them as kind of a independent IoT device or something, 
if you are not familiar with actor model). The actor then parses the measurements, and can then submit them in configurable
intervals to either InfluxDB and/or Azure IoT Hub.   

Note: This is my first open source project, and first project targeting Linux OS, so if I have done something stupid, open an issue, submit a PR or contact me another way.

## Requirements  

* bluez (http://www.bluez.org)

## Future ideas
* Alerts based on temperature high/low, humidity, or movement 
* Home Assistant integration (maybe the alerts that way)
