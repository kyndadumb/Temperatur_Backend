# Temperaturüberwachung

## Dokumentation der Endpunkte

### Sensoren

**Sensoren erstellen:**

Endpunkt: `/api/sensor/create_sensor` (`POST`)

Anfrage:
```json
{
  "serverschrank": "string",
  "adresse": "string",
  "hersteller": "string",
  "max_temperature": 0
}
```

**Alle Sensordaten anzeigen:**

Endpunkt: `/api/Sensor/show_all_sensors` (`GET`)

Antwort:
```json
[
  {
    "sensorID": 3,
    "serverschrank": "test",
    "adresse": "test",
    "hersteller": "test",
    "max_temperature": 50
  }
]
```

**Einzelne Sensordaten anzeigen:**

Endpunkt: `/api/Sensor/{sensor_id}/show_sensor` (`GET`)

Antwort:
```json
{
  "sensorID": 3,
  "serverschrank": "test",
  "adresse": "test",
  "hersteller": "test",
  "max_temperature": 50
}
```

**Sensor löschen:**

Endpunkt: `/api/Sensor/{sensor_id}/delete_sensor` (`POST`)


```json
Der Sensor (ID: {Sensor_ID}) wurde entfernt!
```

**Sensorinformationen ändern:**

Endpunkt: `/api/Sensor/modify_sensor` (`POST`)

Anfrage:
```json
{
  "sensorID": 0,
  "serverschrank": "string",
  "adresse": "string",
  "hersteller": "string",
  "max_temperature": 0
}
```

**Die letzten 10 Temperaturen eines Sensors anzeigen:**

Endpunkt: `/api/Sensor/{sensor_id}/ten_last_temperatures_of_sensor` (`GET`)

Antwort:
```json
[
  13,
  18,
  15,
  17,
  16,
  ...
]
```

**Die letzte Temperatur eines Sensors anzeigen:**

Endpunkt: `/api/Sensor/{sensor_id}/last_temperature_of_sensor` (`GET`)

Antwort:
```json
16
```

**Die höchste Temperatur eines Sensors anzeigen:**

Endpunkt: `/api/Sensor/{sensor_id}/highest_temperature_of_sensor` (`GET`)

Antwort:
```json
18
```

**Die Durchschnittstemperatur eines Sensors anzeigen:**

Endpunkt: `/api/Sensor/{sensor_id}/average_temperature_of_sensor` (`GET`)

Antwort:
```json
15.8
```

### User

**Einen Nutzer registrieren:**

Endpunkt: `/api/User/register_user` (`POST`)

Anfrage:
```json
{
  "username": "string",
  "password": "string",
  "name": "string",
  "phone": "string",
  "isAdmin": true
}
```

**Einloggen mit einem Nutzer:**

Endpunkt: `/api/User/login_user` (`POST`)

Anfrage:
```json
{
  "username": "string",
  "password": "string"
}
```

**Alle Nutzerinformationen anzeigen:**

Endpunkt: `/api/User/show_all_users` (`GET`)

Antwort:
```json
[
  {
    "userID": 1,
    "username": "Dieter69",
    "name": null,
    "phone": "666",
    "isAdmin": "False",
    "lastLogin": "2023-11-04T20:33:13+01:00"
  },
  {
    "userID": 3,
    "username": "Thomas420",
    "name": "'Thomas'",
    "phone": "667",
    "isAdmin": "True",
    "lastLogin": null
  },
  {
    "userID": 4,
    "username": "GüntherTheHero",
    "name": "'Günther'",
    "phone": "668",
    "isAdmin": "False",
    "lastLogin": null
  }
]
```
**Details eines spezifischen Users anzeigen**

Endpunkt: `/api/User/{user_id}/show_user` (`GET`)

Antwort:
```json
{
  "userID": 1,
  "username": "Dieter69",
  "name": null,
  "phone": "666",
  "isAdmin": "False",
  "lastLogin": "2023-11-04T20:33:13+01:00"
}
```
