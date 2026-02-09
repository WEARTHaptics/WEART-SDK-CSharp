
# Change Log
All notable changes to this project will be documented in this file.

## [2.0.1] - 2026-02-09

### Added
- Last RSSI signal strength
- Last sensors calibration date

### Changed
- Documentation
- Changelog file
 
### [2.0.0]

### Added
* New TouchDIVER Pro features
* Custom JSON converted class
* 6 Actuaion Points (Thumb, Index, Middle, Annular, Pinky and Palm)

### Changed
* Replaced Newtonsoft.JSON libraty to native System.text.json for deseralizing and serializing JSON data 
* MiddlewareStatusUpdate is was up to date with new properties for managing WeartApp status and TouchDIVER status
* WeArtTrackingRawDataObject now is providing data from TDPro

### [1.3.0]

### Added
* WeArtAnalogSensorRawDataObject new class that provides analog raw data from thimble's sensors

### Changed
* WeArtRawSensorDataTrackingObject to WeArtTrackingRawDataObject
* SensorData to TrackingRawData


### [1.2.0]
* Add raw sensors data tracking object
* Add middleware status messages and event
* Add connected devices status and event

### [1.1.0]
* Fix connection issue to middleware while offline

### [1.1]
* Add calibration procedure start/stop and listener
* Add new default tracking message and values for closure
* Add basic effect class for haptic feedback
* Remove unused velocity parameters from texture
