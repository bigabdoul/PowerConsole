# PowerConsole

Makes strongly-typed user input collection and validation through the Console
easier, and adds more useful utility methods to it.

# Table of contents

- [What is PowerConsole?](https://github.com/bigabdoul/PowerConsole#what-is-powerconsole)
- [Why PowerConsole?](https://github.com/bigabdoul/PowerConsole#why-powerconsole)
- [Installation](https://github.com/bigabdoul/PowerConsole#installation)
- [Usage](https://github.com/bigabdoul/PowerConsole#usage)
- [Contributing](https://github.com/bigabdoul/PowerConsole#contributing)
- [License](https://github.com/bigabdoul/PowerConsole#license)

## What is PowerConsole? <a id="what-is-powerconsole"></a>

PowerConsole is a .NET Standard project that makes strongly-typed user input
collection and validation through a console easier. Through the **SmartConsole** 
class, it enhances the traditional system's Console by encapsulating complex 
and redundant processing logic, and defining a bunch of utility functions.

## Why PowerConsole? <a id="why-powerconsole"></a>

Almost every beginner tutorial for any server-side programming language makes 
use of the console to write out on a standard input/output screen the famous 
"Hello World!". Why? Because console applications are a great choice for 
learners: they're easy to create and fast to execute. However, when a console 
app requires user interaction such as prompting for their name and password, 
or collecting and validating their inputs against a predefined set of rules, 
then developing efficiently a console app becomes a real pain in the back.

"Great choice for learners" doesn't mean that it's not meant for experienced
developers. In fact, developing advanced console applications are reserved for 
those who actually know what they're doing. So it makes perfectly sense to have
a tool that allows them to be more productive.

Developing efficient console applications is a daunting task. This is mostly 
due to the fact that the Console class in .NET's System namespace is a static 
class by design and as such does not retain state, thus making it difficult to 
collect a set of related data. That's where SmartConsole steps in: it 
encapsulates methods and data required to build an interactive console 
application that seamlessly enforces complex business logic.

## Installation <a id="installation"></a>

Create a new .NET (Core or Framework) Console application and install the package
from NuGet.

- If you are using **Visual Studio 2019** or later, navigate to the menu 
*Tools > NuGet Package Manager > Package Manager Console* and type the following:

    `Install-Package PowerConsole`

- If you are using **Visual Studio Code** or the **dotnet** command line tool, open a
command prompt terminal in your current project folder and type the following:

    `dotnet add package PowerConsole`

## Usage <a id="usage"></a>

To see how you can use `PowerConsole` please read the gist 
[An introduction to using PowerConsole](https://gist.github.com/bigabdoul/c60e814b8f497be43e612b62ca1a15db).

## Contributing <a id="contributing"></a>

Please read the [CONTRIBUTING.md](https://github.com/bigabdoul/PowerConsole/blob/master/CONTRIBUTING.md)
file for contribution guide lines.

## License <a id="license"></a>

This project is licensed under the MIT license terms. Please read the 
[LICENSE.txt](https://github.com/bigabdoul/PowerConsole/blob/master/LICENSE.txt)
file.