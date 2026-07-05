# PulseHub Architecture

## Purpose

PulseHub is a telemetry platform for collecting, normalizing, storing and later visualizing data from different sources.

The first version focuses on simple and understandable data sources, such as mock sensors, weather data, smart home devices and later MQTT/Home Assistant integrations.

The long-term idea is to build a system that can evolve into an industrial-style monitoring platform.

---

## Core Data Flow

```text
Source
  -> Collector
  -> Parser
  -> Normalizer
  -> Measurement
  -> Database
  -> API
  -> SignalR
  -> UI
```

##  Main Concepts
Source

A Source describes where data comes from.

Examples:

Mock Sensor
Weather API
Shelly Device
MQTT Topic
Home Assistant
Virtual PLC

A Source does not store measurements itself. It only describes the origin of data.

##  Collector

A Collector knows how to get raw data from a specific Source.

Examples:

HTTP Collector
MQTT Collector
Mock Collector
File Collector

Collectors return raw data.

##  Parser

A Parser understands the format of raw data.

Examples:

JSON Parser
Plain Text Parser
Custom Shelly Parser
Custom Weather Parser

The parser converts raw data into structured intermediate data.

##  Normalizer

The Normalizer converts parsed data into a unified internal format.

This is important because different sources may return different field names, formats and units.

Example:

Shelly: { "power": 38.2 }
Weather API: { "temperature": 21.5 }

Normalized:
MetricName
Value
Unit
Timestamp
SourceId
Measurement

##  A Measurement is the main stored telemetry value.

Example:

Device: Desk Plug
Metric: Power
Value: 38.2
Unit: W
Timestamp: 2026-07-05T18:00:00
First MVP

The first MVP should be intentionally simple.

It should support:

Create a Source
Generate mock measurements
Store measurements in SQL Server
Read latest measurements via API
Read measurement history via API

No authentication in the first version.

No frontend in the first version.

No microservices in the first version.

##  Architectural Principles
Start as a modular monolith

The project starts as a modular monolith because it is easier to develop, debug and refactor.

Microservices should be introduced only when clear module boundaries exist.

Keep external data separate from internal data

Raw data from sources should not be used directly as domain data.

External formats must be parsed and normalized first.

Avoid unnecessary abstractions

The project should not introduce patterns only for the sake of using patterns.

Every abstraction should solve a real problem.