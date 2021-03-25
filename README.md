# README #

![Fransom Logo](/images/Fransom.png)

## Fransom - Fraktal's Ransomware Emulator. 

## What is this repository for?

* Command-line executable that will emulate common ransomware functions for the purpose of testing endpoint detection and response tools.
* Version: 0.6
* Type "help" (without quotes) inside the emulator prompt to get a list of all the available options.

## About 

* About the tool: https://blog.fraktal.fi/
* About the authors: https://www.fraktal.fi 

## How do I get set up?

* Open the .sln solution file in Visual Studio (developed with VS 2019 and VS 2017).
* To add/modify functions:
	* Add the corresponding option in the "options" class;
	* Add the corresponding option in the "Run" function;
	* Add the corresponding option in the switch inside the main function;
	* Create the corresponding function.
* Dependencies:
	* ILMerge NuGet package.
	* ILMerge.MSBuild.Tasks NuGet package.
	* Figgle NuGet package.

## Risk considerations

The tool is not designed as destructive. Still, we cannot anticipate how various endpoint protection tools respond to running it. For this reason we do not recommend running the tool in any critical or production system.

## Licenses and acknowledgements
   
Fransom incorporates code and functionality from the following open source offensive .NET tools:

 * Process injection mechanisms are from [C# Memory Injection Examples project](https://github.com/pwndizzle/c-sharp-memory-injection) by [pwndizzle](https://twitter.com/pwndizzle). 
 * LSASS dumping implementation is from https://github.com/GhostPack/SharpDump by [harmj0y](https://twitter.com/harmj0y). SharpDump is licensed under the BSD 3-Clause license, available [here](https://raw.githubusercontent.com/GhostPack/SharpDump/master/LICENSE).
 * calc.exe shellcode is from [win-exec-calc-shellcode](https://github.com/peterferrie/win-exec-calc-shellcode) by Berend-Jan "SkyLined" Wever and Peter Ferrie. [Copyright](https://raw.githubusercontent.com/peterferrie/win-exec-calc-shellcode/master/COPYRIGHT.txt).
