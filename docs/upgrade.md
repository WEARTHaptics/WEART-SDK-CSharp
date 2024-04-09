# Update from older SDK versions

To update your application to the latest SDK, download and extract the C# sdk archive, then copy the source/header files in the same place as the older SDK version.

The new version includes additional files, so it's necessary to add them to the project in order to avoid linking errors.

/note 

This can be done on Visual Studio by right-clicking on the solution, then clicking on Add -> Existing Item and selecting all the SDK files.
On other systems (e.g. cmake) the procedure might be different.

## Code Migration

The new SDK version is mostly compatible with the older versions.
We have introduced new components that could be used in your project.

## Dependencies
The new SDK version has the following dependencies:
* Newtonsoft.Json version >= 13.0.0 (currently tested with version 13.0.3)

The dependencies can be found and installed by using the nuget package manager.
