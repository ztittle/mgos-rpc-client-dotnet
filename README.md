# Mongoose-OS RPC Client
This library implements the [Mongoose-OS RPC Protocol](https://mongoose-os.com/docs/mos/userguide/rpc.md) for use in .NET Applications.

It has been tested against AWS IoT and .NET Core, but it should work with any MQTT broker and .NET Application that supports .NET Standard 2.0 and above.

# Features

* Invoke RPC methods on devices running [Mongoose-OS](https://mongoose-os.com/)
* Register RPC handlers for simulating a device running Mongoose-OS
* Built-in support for native Mongoose-OS RPC services

# Dependencies

The library depends on the following

* [System.Text.Json](https://www.nuget.org/packages/System.Text.Json) for request and response serialization.
* [MQTTnet](https://github.com/chkr1011/MQTTnet) for connecting to an MQTT broker.

# Install

Install via nuget package `MongooseOS.Rpc`.

# Usage

```csharp
    var mqttFactory = new MQTTnet.MqttFactory();

    var clientPfx = File.ReadAllBytes("client.pfx");
    var caCert = File.ReadAllBytes("cacert.crt");

    var mgosRpcClient = new MgosRpcClient(
        mqttFactory.CreateMqttClient(),
        mqttEndpoint: "mqttbroker:8883",
        clientId: "myclientId",
        clientPfx: clientPfx,
        caCert: caCert);

    await mgosRpcClient.ConnectAsync();

    // Get System Info (built in)
    var sysRpc = new SysRpc(mgosRpcClient);
    var sysInfo = await sysRpc.GetInfo("esp8266_ABCDEF");

    // Call custom method
    var result = await mgosRpcClient.SendAsync<int>("esp8266_ABCDEF", "Sum", new 
    {
        a = 1,
        b = 2
    });
```

# Todo

* Add support for all native Mongoose-OS RPC Services
* Add unit tests
