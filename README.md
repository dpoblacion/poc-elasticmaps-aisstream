# Description

This repo contains a POC to display information on the Kibana map by consuming real-time data from a WebSocket. For real-time data, we use vessel positions from [aisstream.io](https://aisstream.io)

## Prerequisites

* You will need to sign up for an AISStream API key at [aisstream.io](https://aisstream.io/authenticate) to recieve messages from the vessels in realtime.
* Docker CE and docker compose installed.

## Quick Start

1. Change asistream api key in env file.
2. Run `docker compose up`
3. Load kibana using http://localhost:5601/ (credentials in env file)
3. Import map.ndjson on ELK 


https://github.com/dpoblacion/poc-elasticmaps-aisstream/assets/4161120/f64ba1e5-dac9-4894-a416-7fdd11988aa3

