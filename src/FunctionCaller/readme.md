### Creating a ZipTemplate.zip file

A ZipTemplate.zip file contains a set of default libraries that are included in the function package.

To create a ZipTemplate.zip file:

1. Publish the `FunctionHandler` project.
2. Copy `Newtonsoft.Json.dll` and `System.Runtime.Serialization.Primitives.dll` into the publish output folder, (usually `... \bin\Release\netcoreapp1.0`). These files are needed for the Json.NET library.
3. Copy other libraries you want access to in the AWS Lambda environment.
4. Zip up the files and name the zip file `ZipTemplate.zip`.
5. Replace the ZipTemplate.zip file in the `... \PackageBuilder\AppData` folder.
