## Azure Style C# Functions for AWS Lambda ##

This is a proof of concept project that demonstrates AWS Lambda C# functions, in the style of Azure C# functions, that are easy to create online using a code editing experience that's superior to the Azure Portal editor.

Write your AWS C# function online with desktop-like code completion and compilation error feedback, and build your function package at the click of a button!

For a live demonstration, see [csharpaws.com](http://csharpaws.com).

### Caveats

The code editor is powered by [C# Pad](http://csharppad.com) which targets the .NET framework, whereas the lambda function package is compiled against .NET Core. Therefore the editor may suggest .NET framework APIs that do not match .NET Core APIs. 

### Project Plan

There is no project plan as this is a low-key proof of concept project.
In the distant future, the project may morph in the following directions:

1. Allow users to directly create/update Lambda functions in their AWS account without needing a download step.
2. Support MVC/Web API/RazorPages for full-featured and easy online web development powered by AWS Lambda.
3. Deconstruction of this project so it can [run on AWS Lambda](https://aws.amazon.com/serverless/build-a-web-app/) itself. 

### License

MIT License
